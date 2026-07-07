using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Hofinsoft.Mdg.Services
{
    /// <summary>
    /// Fuzzy-Similarity Index engine. Checks GoldenMasterCatalog and staging requests
    /// for duplicates based on attribute hash and plant-specific rules.
    /// </summary>
    public class DuplicateDetector
    {
        private readonly NomcatDbContext _db;
        private readonly GeminiService _gemini;

        public DuplicateDetector(NomcatDbContext db, GeminiService gemini)
        {
            _db = db;
            _gemini = gemini;
        }

        /// <summary>
        /// Generates a deterministic SHA-256 hash from noun + modifier + sorted attribute values.
        /// This serves as the Fuzzy-Similarity Index key.
        /// </summary>
        public static string ComputeHash(string noun, string modifier, Dictionary<string, string> attributes)
        {
            // Normalize: uppercase, sort keys alphabetically, trim values
            var normalized = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in attributes)
            {
                // Exclude plant since it's a structural field, not a catalog classification attribute
                if (kvp.Key.Equals("Plant", StringComparison.OrdinalIgnoreCase)) continue;
                
                normalized[kvp.Key.Trim().ToUpper()] = kvp.Value.Trim().ToUpper();
            }

            var payload = $"{noun.Trim().ToUpper()}|{modifier.Trim().ToUpper()}|{JsonSerializer.Serialize(normalized)}";
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexStringLower(hashBytes)[..16]; // Short hash for readability
        }

        /// <summary>
        /// Full dedup check based on RequestType.
        /// </summary>
        public DuplicateCheckResult CheckForDuplicate(string duplicateHash, string plant, string requestType)
        {
            var cleanPlant = plant.Trim().ToUpper();
            var cleanType = requestType.Trim();

            if (cleanType == "Plant_Extension")
            {
                // Rule 1: The material must exist in the Golden Catalog under SOME plant
                var existsInGoldenAnyPlant = _db.GoldenMasterCatalog.Any(g => g.DuplicateHash == duplicateHash);
                if (!existsInGoldenAnyPlant)
                {
                    return new DuplicateCheckResult
                    {
                        IsDuplicate = true,
                        Source = "ValidationError",
                        Message = "Cannot perform Plant Extension: Material definition does not exist in Golden Master Catalog."
                    };
                }

                // Rule 2: The material must NOT already exist in Golden Catalog for the target plant
                var existsInGoldenTargetPlant = _db.GoldenMasterCatalog
                    .Any(g => g.DuplicateHash == duplicateHash && g.Plant == cleanPlant);
                if (existsInGoldenTargetPlant)
                {
                    var match = _db.GoldenMasterCatalog.First(g => g.DuplicateHash == duplicateHash && g.Plant == cleanPlant);
                    return new DuplicateCheckResult
                    {
                        IsDuplicate = true,
                        Source = "GoldenMasterCatalog",
                        ExistingRef = match.MaterialNumber,
                        Message = $"Duplicate extension: Material is already active at plant '{cleanPlant}' (SAP Code: {match.MaterialNumber})."
                    };
                }

                // Rule 3: The material must NOT be pending approval for the target plant in staging
                var existsInStagingTargetPlant = _db.ItemRequests
                    .Any(r => r.DuplicateHash == duplicateHash 
                           && r.Plant == cleanPlant 
                           && r.ApprovalStatus != "Rejected" 
                           && r.ApprovalStatus != "Duplicated");
                if (existsInStagingTargetPlant)
                {
                    var match = _db.ItemRequests.First(r => r.DuplicateHash == duplicateHash 
                                                         && r.Plant == cleanPlant 
                                                         && r.ApprovalStatus != "Rejected" 
                                                         && r.ApprovalStatus != "Duplicated");
                    return new DuplicateCheckResult
                    {
                        IsDuplicate = true,
                        Source = "StagingPipeline",
                        ExistingRef = match.RequestRefNo,
                        Message = $"Duplicate extension: An extension request for plant '{cleanPlant}' is already pending in Staging ({match.RequestRefNo})."
                    };
                }

                // If it exists in Golden under another plant, and is not yet in the target plant, it's valid!
                return new DuplicateCheckResult { IsDuplicate = false };
            }
            else
            {
                // Standard requests (Single, Multiple, Modification) require global catalog uniqueness
                var existsInGolden = _db.GoldenMasterCatalog.Any(g => g.DuplicateHash == duplicateHash);
                if (existsInGolden)
                {
                    var match = _db.GoldenMasterCatalog.First(g => g.DuplicateHash == duplicateHash);
                    return new DuplicateCheckResult
                    {
                        IsDuplicate = true,
                        Source = "GoldenMasterCatalog",
                        ExistingRef = match.MaterialNumber,
                        Message = $"Duplicate blocked: Material already exists in Golden Catalog (SAP Code: {match.MaterialNumber}). Use 'Plant Extension' to assign it to plant '{cleanPlant}'."
                    };
                }

                var existsInStaging = _db.ItemRequests
                    .Any(r => r.DuplicateHash == duplicateHash 
                           && r.ApprovalStatus != "Rejected" 
                           && r.ApprovalStatus != "Duplicated");
                if (existsInStaging)
                {
                    var match = _db.ItemRequests.First(r => r.DuplicateHash == duplicateHash 
                                                         && r.ApprovalStatus != "Rejected" 
                                                         && r.ApprovalStatus != "Duplicated");
                    return new DuplicateCheckResult
                    {
                        IsDuplicate = true,
                        Source = "StagingPipeline",
                        ExistingRef = match.RequestRefNo,
                        Message = $"Duplicate blocked: An identical item is already pending in the Staging pipeline under request '{match.RequestRefNo}'."
                    };
                }
            }

            return new DuplicateCheckResult { IsDuplicate = false };
        }

        /// <summary>
        /// Semantic duplicate check using Gemini text embeddings and cosine similarity.
        /// Compares the short description against all Golden Master Records with stored embeddings.
        /// </summary>
        public async Task<SemanticDuplicateResult> SemanticDuplicateCheckAsync(string shortDescription, string plant)
        {
            if (!_gemini.IsConfigured)
            {
                return new SemanticDuplicateResult
                {
                    IsBlocked = false,
                    IsSuspicious = false,
                    Message = "Semantic check skipped (AI not configured)"
                };
            }

            // Generate embedding for the incoming description
            float[]? queryEmbedding = null;
            try
            {
                queryEmbedding = await _gemini.GenerateEmbeddingAsync(shortDescription);
            }
            catch (Exception ex)
            {
                return new SemanticDuplicateResult
                {
                    IsBlocked = false,
                    IsSuspicious = false,
                    Message = $"Semantic check skipped (embedding error: {ex.Message})"
                };
            }

            if (queryEmbedding == null)
            {
                return new SemanticDuplicateResult
                {
                    IsBlocked = false,
                    IsSuspicious = false,
                    Message = "Semantic check skipped (failed to generate query embedding)"
                };
            }

            // Load all golden records that have stored embeddings
            var goldenRecords = await _db.GoldenMasterCatalog
                .Where(g => g.EmbeddingVector != null && g.EmbeddingVector != "")
                .Select(g => new
                {
                    g.MaterialNumber,
                    g.ShortDescription,
                    g.Plant,
                    g.EmbeddingVector
                })
                .ToListAsync();

            if (goldenRecords.Count == 0)
            {
                return new SemanticDuplicateResult
                {
                    IsBlocked = false,
                    IsSuspicious = false,
                    Message = "No embedded golden records available for semantic comparison."
                };
            }

            // Compute cosine similarity against each record
            var similarities = new List<(string MaterialNumber, string ShortDescription, string Plant, double Similarity)>();

            foreach (var record in goldenRecords)
            {
                try
                {
                    var storedEmbedding = JsonSerializer.Deserialize<float[]>(record.EmbeddingVector!);
                    if (storedEmbedding == null || storedEmbedding.Length == 0) continue;

                    var similarity = CosineSimilarity(queryEmbedding, storedEmbedding);
                    similarities.Add((record.MaterialNumber, record.ShortDescription, record.Plant, similarity));
                }
                catch
                {
                    // Skip records with malformed embedding data
                }
            }

            if (similarities.Count == 0)
            {
                return new SemanticDuplicateResult
                {
                    IsBlocked = false,
                    IsSuspicious = false,
                    Message = "No valid embeddings found for comparison."
                };
            }

            // Sort by similarity descending and take top 3
            var topMatches = similarities
                .OrderByDescending(s => s.Similarity)
                .Take(3)
                .Select(s => new AiSearchResult
                {
                    MaterialNumber = s.MaterialNumber,
                    ShortDescription = s.ShortDescription,
                    Plant = s.Plant,
                    Similarity = Math.Round(s.Similarity, 4)
                })
                .ToList();

            var highest = topMatches.First().Similarity;

            var result = new SemanticDuplicateResult
            {
                HighestSimilarity = highest,
                TopMatches = topMatches
            };

            if (highest > 0.92)
            {
                result.IsBlocked = true;
                result.IsSuspicious = true;
                result.Message = $"Near-duplicate detected (similarity: {highest:P1}). " +
                                 $"Closest match: {topMatches.First().MaterialNumber} — '{topMatches.First().ShortDescription}'.";
            }
            else if (highest > 0.80)
            {
                result.IsBlocked = false;
                result.IsSuspicious = true;
                result.Message = $"Potential similar materials found (similarity: {highest:P1}). " +
                                 $"Closest match: {topMatches.First().MaterialNumber} — '{topMatches.First().ShortDescription}'.";
            }
            else
            {
                result.IsBlocked = false;
                result.IsSuspicious = false;
                result.Message = "No semantically similar materials found.";
            }

            return result;
        }

        /// <summary>
        /// Computes cosine similarity between two vectors.
        /// Returns a value between -1 and 1, where 1 means identical direction.
        /// </summary>
        private static double CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0;

            double dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * (double)b[i];
                magA += a[i] * (double)a[i];
                magB += b[i] * (double)b[i];
            }

            var denominator = Math.Sqrt(magA) * Math.Sqrt(magB);
            return denominator == 0 ? 0 : dot / denominator;
        }
    }

    public class DuplicateCheckResult
    {
        public bool IsDuplicate { get; set; }
        public string Source { get; set; } = string.Empty;
        public string ExistingRef { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
