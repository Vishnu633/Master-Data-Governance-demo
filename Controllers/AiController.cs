using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Services;
using Hofinsoft.Mdg.Models;
using Hofinsoft.Mdg.Models.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Hofinsoft.Mdg.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly GeminiService _gemini;
        private readonly NomcatDbContext _db;
        private readonly DescriptionEngine _descEngine;
        private readonly ILogger<AiController> _logger;

        public AiController(
            GeminiService gemini,
            NomcatDbContext db,
            DescriptionEngine descEngine,
            ILogger<AiController> logger)
        {
            _gemini = gemini;
            _db = db;
            _descEngine = descEngine;
            _logger = logger;
        }

        [HttpPost("classify")]
        public async Task<IActionResult> Classify([FromBody] AiClassifyRequest request)
        {
            if (!_gemini.IsConfigured)
            {
                return StatusCode(503, new { error = "AI service not configured. Set GEMINI_API_KEY environment variable." });
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest("Description cannot be empty.");
            }

            try
            {
                // Fetch list of active noun/modifier combos in system
                var profiles = await _db.AttributeMaster
                    .Select(a => new { a.Noun, a.Modifier })
                    .Distinct()
                    .ToListAsync();

                var profileStrings = profiles.Select(p => $"{p.Noun}/{p.Modifier}").ToList();

                var aiResult = await _gemini.ClassifyMaterialAsync(request.Description, profileStrings);
                if (aiResult == null)
                {
                    return UnprocessableEntity("AI failed to extract structured attributes from description.");
                }

                // Normalise Noun/Modifier casing
                aiResult.Noun = aiResult.Noun.Trim().ToUpper();
                aiResult.Modifier = aiResult.Modifier.Trim().ToUpper();

                // Ensure the noun/modifier exists in the database
                var profileExists = profiles.Any(p => p.Noun == aiResult.Noun && p.Modifier == aiResult.Modifier);
                if (!profileExists)
                {
                    _logger.LogWarning("AI classified as {Noun}/{Modifier} but it is not seeded in database.", aiResult.Noun, aiResult.Modifier);
                    return UnprocessableEntity($"AI suggested classification {aiResult.Noun}/{aiResult.Modifier} which does not exist.");
                }

                // Align/normalize attributes to match database attribute keys
                var dbAttributes = await _db.AttributeMaster
                    .Where(a => a.Noun == aiResult.Noun && a.Modifier == aiResult.Modifier)
                    .Select(a => a.AttributeName)
                    .ToListAsync();

                var normalizedAttributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var dbAttr in dbAttributes)
                {
                    var matchedKey = aiResult.Attributes.Keys.FirstOrDefault(k => IsAttributeMatch(k, dbAttr, dbAttributes));
                    if (matchedKey != null)
                    {
                        normalizedAttributes[dbAttr] = aiResult.Attributes[matchedKey];
                    }
                    else
                    {
                        normalizedAttributes[dbAttr] = "";
                    }
                }
                aiResult.Attributes = normalizedAttributes;

                // Generate standard descriptions using our DescriptionEngine template rules
                var shortDesc = _descEngine.GenerateShortDescription(aiResult.Noun, aiResult.Modifier, aiResult.Attributes);
                aiResult.GeneratedDescription = shortDesc;

                return Ok(aiResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI material classification endpoint");
                return StatusCode(500, "An internal error occurred during classification.");
            }
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] AiSearchRequest request)
        {
            if (!_gemini.IsConfigured)
            {
                return StatusCode(503, new { error = "AI service not configured. Set GEMINI_API_KEY environment variable." });
            }

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest("Search query cannot be empty.");
            }

            try
            {
                // Generate embedding for query
                var queryEmbedding = await _gemini.GenerateEmbeddingAsync(request.Query);
                if (queryEmbedding == null)
                {
                    return StatusCode(500, "Failed to compute embedding vector for search query.");
                }

                // Fetch golden records with embeddings
                var records = await _db.GoldenMasterCatalog
                    .Where(g => g.EmbeddingVector != null && g.EmbeddingVector != "")
                    .ToListAsync();

                if (records.Count == 0)
                {
                    // Fall back to case-insensitive substring matches if database is not embedded
                    var fallbackMatches = await _db.GoldenMasterCatalog
                        .Where(g => g.MaterialNumber.ToLower().Contains(request.Query.ToLower()) ||
                                    g.ShortDescription.ToLower().Contains(request.Query.ToLower()))
                        .Take(5)
                        .Select(g => new AiSearchResult
                        {
                            MaterialNumber = g.MaterialNumber,
                            ShortDescription = g.ShortDescription,
                            Plant = g.Plant,
                            Similarity = 0.5 // Default fallback similarity
                        })
                        .ToListAsync();

                    return Ok(fallbackMatches);
                }

                var matches = new List<AiSearchResult>();

                foreach (var record in records)
                {
                    try
                    {
                        var storedEmbedding = JsonSerializer.Deserialize<float[]>(record.EmbeddingVector!);
                        if (storedEmbedding == null || storedEmbedding.Length == 0) continue;

                        var similarity = CosineSimilarity(queryEmbedding, storedEmbedding);
                        matches.Add(new AiSearchResult
                        {
                            MaterialNumber = record.MaterialNumber,
                            ShortDescription = record.ShortDescription,
                            Plant = record.Plant,
                            Similarity = Math.Round(similarity, 4)
                        });
                    }
                    catch
                    {
                        // Skip corrupt embeddings
                    }
                }

                // Return top 5 matches
                var topMatches = matches
                    .OrderByDescending(m => m.Similarity)
                    .Take(5)
                    .ToList();

                return Ok(topMatches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI semantic search endpoint");
                return StatusCode(500, "An internal error occurred during semantic search.");
            }
        }

        [HttpPost("classify-image")]
        public async Task<IActionResult> ClassifyImage(IFormFile file)
        {
            if (!_gemini.IsConfigured)
            {
                return StatusCode(503, new { error = "AI service not configured. Set GEMINI_API_KEY environment variable." });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No image file provided.");
            }

            try
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var imageBytes = ms.ToArray();

                var profiles = await _db.AttributeMaster
                    .Select(a => new { a.Noun, a.Modifier })
                    .Distinct()
                    .ToListAsync();
                var profileStrings = profiles.Select(p => $"{p.Noun}/{p.Modifier}").ToList();

                var aiResult = await _gemini.ClassifyMaterialImageAsync(imageBytes, file.ContentType, profileStrings);
                if (aiResult == null)
                {
                    return UnprocessableEntity("AI failed to classify image into a profile.");
                }

                aiResult.Noun = aiResult.Noun.Trim().ToUpper();
                aiResult.Modifier = aiResult.Modifier.Trim().ToUpper();

                var profileExists = profiles.Any(p => p.Noun == aiResult.Noun && p.Modifier == aiResult.Modifier);
                if (!profileExists)
                {
                    _logger.LogWarning("AI image classified as {Noun}/{Modifier} but it is not seeded in database.", aiResult.Noun, aiResult.Modifier);
                    return UnprocessableEntity($"AI suggested classification {aiResult.Noun}/{aiResult.Modifier} which does not exist.");
                }

                var dbAttributes = await _db.AttributeMaster
                    .Where(a => a.Noun == aiResult.Noun && a.Modifier == aiResult.Modifier)
                    .Select(a => a.AttributeName)
                    .ToListAsync();

                var normalizedAttributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var dbAttr in dbAttributes)
                {
                    var matchedKey = aiResult.Attributes.Keys.FirstOrDefault(k => IsAttributeMatch(k, dbAttr, dbAttributes));
                    if (matchedKey != null)
                    {
                        normalizedAttributes[dbAttr] = aiResult.Attributes[matchedKey];
                    }
                    else
                    {
                        normalizedAttributes[dbAttr] = "";
                    }
                }
                aiResult.Attributes = normalizedAttributes;

                var shortDesc = _descEngine.GenerateShortDescription(aiResult.Noun, aiResult.Modifier, aiResult.Attributes);
                aiResult.GeneratedDescription = shortDesc;

                return Ok(aiResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI material image classification endpoint");
                return StatusCode(500, "An internal error occurred during image classification.");
            }
        }

        [HttpPost("audit")]
        public async Task<IActionResult> Audit([FromBody] AiAuditRequest request)
        {
            if (!_gemini.IsConfigured)
            {
                return StatusCode(503, new { error = "AI service not configured." });
            }

            if (string.IsNullOrWhiteSpace(request.Noun) || string.IsNullOrWhiteSpace(request.Modifier))
            {
                return BadRequest("Noun and Modifier cannot be empty.");
            }

            try
            {
                var auditReport = await _gemini.AuditMaterialAsync(request.Noun, request.Modifier, request.Attributes);
                return Ok(new { auditReport });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI material audit endpoint");
                return StatusCode(500, "An internal error occurred during auditing.");
            }
        }

        [HttpPost("bulk-clean")]
        public async Task<IActionResult> BulkClean([FromBody] AiBulkCleanRequest request)
        {
            if (!_gemini.IsConfigured)
            {
                return StatusCode(503, new { error = "AI service not configured." });
            }

            if (request.Descriptions == null || request.Descriptions.Count == 0)
            {
                return BadRequest("No legacy descriptions provided.");
            }

            try
            {
                var profiles = await _db.AttributeMaster
                    .Select(a => new { a.Noun, a.Modifier })
                    .Distinct()
                    .ToListAsync();
                var profileStrings = profiles.Select(p => $"{p.Noun}/{p.Modifier}").ToList();

                var rawResults = await _gemini.BulkCleanMaterialsAsync(request.Descriptions, profileStrings);
                var finalResults = new List<AiClassificationResult>();

                foreach (var aiResult in rawResults)
                {
                    try
                    {
                        aiResult.Noun = aiResult.Noun.Trim().ToUpper();
                        aiResult.Modifier = aiResult.Modifier.Trim().ToUpper();

                        var matchedProfile = profiles.FirstOrDefault(p => p.Noun == aiResult.Noun && p.Modifier == aiResult.Modifier);
                        if (matchedProfile == null)
                        {
                            matchedProfile = profiles.FirstOrDefault(p => 
                                string.Equals(p.Noun, aiResult.Noun, StringComparison.OrdinalIgnoreCase) && 
                                string.Equals(p.Modifier, aiResult.Modifier, StringComparison.OrdinalIgnoreCase));
                        }

                        if (matchedProfile != null)
                        {
                            aiResult.Noun = matchedProfile.Noun;
                            aiResult.Modifier = matchedProfile.Modifier;

                            var dbAttributes = await _db.AttributeMaster
                                .Where(a => a.Noun == aiResult.Noun && a.Modifier == aiResult.Modifier)
                                .Select(a => a.AttributeName)
                                .ToListAsync();

                            var normalizedAttributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var dbAttr in dbAttributes)
                            {
                                var matchedKey = aiResult.Attributes.Keys.FirstOrDefault(k => IsAttributeMatch(k, dbAttr, dbAttributes));
                                if (matchedKey != null)
                                {
                                    normalizedAttributes[dbAttr] = aiResult.Attributes[matchedKey];
                                }
                                else
                                {
                                    normalizedAttributes[dbAttr] = "";
                                }
                            }
                            aiResult.Attributes = normalizedAttributes;

                            var shortDesc = _descEngine.GenerateShortDescription(aiResult.Noun, aiResult.Modifier, aiResult.Attributes);
                            aiResult.GeneratedDescription = shortDesc;

                            finalResults.Add(aiResult);
                        }
                    }
                    catch
                    {
                        // Skip unparseable results in bulk
                    }
                }

                return Ok(finalResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI bulk clean endpoint");
            }
        }

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

        private static bool IsAttributeMatch(string key, string dbAttr, List<string> allDbAttrs)
        {
            if (string.Equals(key, dbAttr, StringComparison.OrdinalIgnoreCase))
                return true;

            var kClean = CleanKey(key);
            var dClean = CleanKey(dbAttr);

            if (string.Equals(kClean, dClean, StringComparison.OrdinalIgnoreCase))
                return true;

            kClean = ApplySynonyms(kClean);
            dClean = ApplySynonyms(dClean);

            if (string.Equals(kClean, dClean, StringComparison.OrdinalIgnoreCase))
                return true;

            // Handle ID / OD abbreviations
            if (string.Equals(dbAttr, "Inside_Diameter", StringComparison.OrdinalIgnoreCase) && 
                (string.Equals(key, "id", StringComparison.OrdinalIgnoreCase) || 
                 string.Equals(key, "inside", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(key, "inner", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(key, "inside_diameter", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(key, "inner_diameter", StringComparison.OrdinalIgnoreCase)))
                return true;

            if (string.Equals(dbAttr, "Outside_Diameter", StringComparison.OrdinalIgnoreCase) && 
                (string.Equals(key, "od", StringComparison.OrdinalIgnoreCase) || 
                 string.Equals(key, "outside", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(key, "outer", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(key, "outside_diameter", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(key, "outer_diameter", StringComparison.OrdinalIgnoreCase)))
                return true;

            // Handle generic "SIZE" or "DIAMETER" key mapping
            if (string.Equals(key, "size", StringComparison.OrdinalIgnoreCase) || string.Equals(key, "diameter", StringComparison.OrdinalIgnoreCase))
            {
                // For bearings: map to Inside_Diameter if it exists
                if (string.Equals(dbAttr, "Inside_Diameter", StringComparison.OrdinalIgnoreCase))
                    return true;

                // For bolts/studs: map to Thread if it exists (e.g. M12 size)
                if (string.Equals(dbAttr, "Thread", StringComparison.OrdinalIgnoreCase) && !allDbAttrs.Any(a => string.Equals(a, "Inside_Diameter", StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            if (string.Equals(dbAttr, "Length", StringComparison.OrdinalIgnoreCase) && 
                (string.Equals(key, "lg", StringComparison.OrdinalIgnoreCase) || string.Equals(key, "len", StringComparison.OrdinalIgnoreCase)))
                return true;

            if (string.Equals(dbAttr, "Thickness", StringComparison.OrdinalIgnoreCase) && 
                string.Equals(key, "thk", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(dbAttr, "Width", StringComparison.OrdinalIgnoreCase) && 
                string.Equals(key, "w", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(dbAttr, "Height", StringComparison.OrdinalIgnoreCase) && 
                string.Equals(key, "h", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static string CleanKey(string val)
        {
            if (string.IsNullOrEmpty(val)) return "";
            return val.Replace("_", "").Replace(" ", "").Replace("-", "").ToLower();
        }

        private static string ApplySynonyms(string val)
        {
            if (val == "size") return val;
            return val
                .Replace("inner", "inside")
                .Replace("outer", "outside")
                .Replace("dia", "diameter")
                .Replace("size", "")
                .Replace("grade", "");
        }
    }
}
