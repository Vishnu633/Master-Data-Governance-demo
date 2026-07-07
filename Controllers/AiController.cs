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
}
