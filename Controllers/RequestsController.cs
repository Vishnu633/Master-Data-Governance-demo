using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Models;
using Hofinsoft.Mdg.Models.Dto;
using Hofinsoft.Mdg.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hofinsoft.Mdg.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly NomcatDbContext _db;
        private readonly TicketGenerator _ticketGen;
        private readonly DescriptionEngine _descEngine;
        private readonly DuplicateDetector _dupDetector;
        private readonly LifecycleRouter _lifecycleRouter;
        private readonly ExportBridge _exportBridge;
        private readonly GeminiService _gemini;

        public RequestsController(
            NomcatDbContext db,
            TicketGenerator ticketGen,
            DescriptionEngine descEngine,
            DuplicateDetector dupDetector,
            LifecycleRouter lifecycleRouter,
            ExportBridge exportBridge,
            GeminiService gemini)
        {
            _db = db;
            _ticketGen = ticketGen;
            _descEngine = descEngine;
            _dupDetector = dupDetector;
            _lifecycleRouter = lifecycleRouter;
            _exportBridge = exportBridge;
            _gemini = gemini;
        }

        /// <summary>
        /// POST /api/requests/submit
        /// Submit a new material request. Runs Stage 1 automation immediately.
        /// </summary>
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitRequest([FromBody] MaterialRequestDto dto)
        {
            var cleanPlant = string.IsNullOrWhiteSpace(dto.Plant) ? "PLT1" : dto.Plant.Trim().ToUpper();

            // Validate noun/modifier exists in AttributeMaster
            var schemaExists = await _db.AttributeMaster
                .AnyAsync(a => a.Noun == dto.Noun.Trim().ToUpper() && a.Modifier == dto.Modifier.Trim().ToUpper());

            if (!schemaExists)
                return BadRequest($"No attribute schema found for {dto.Noun}/{dto.Modifier}.");

            // Validate mandatory attributes
            var mandatoryAttrs = await _db.AttributeMaster
                .Where(a => a.Noun == dto.Noun.Trim().ToUpper()
                         && a.Modifier == dto.Modifier.Trim().ToUpper()
                         && a.MandatoryIndicator == "Y")
                .Select(a => a.AttributeName)
                .ToListAsync();

            var missingAttrs = mandatoryAttrs
                .Where(attr => !dto.Attributes.ContainsKey(attr) || string.IsNullOrWhiteSpace(dto.Attributes[attr]))
                .ToList();

            if (missingAttrs.Count > 0)
                return BadRequest($"Missing mandatory attributes: {string.Join(", ", missingAttrs)}");

            // Compute duplicate hash
            var hash = DuplicateDetector.ComputeHash(dto.Noun, dto.Modifier, dto.Attributes);

            // STAGE 1 AUTOMATION: Fuzzy-Similarity Index check (with plant and type context)
            var dupResult = _dupDetector.CheckForDuplicate(hash, cleanPlant, dto.RequestType);
            if (dupResult.IsDuplicate)
            {
                return Conflict(new
                {
                    status = "DUPLICATED",
                    message = dupResult.Message,
                    source = dupResult.Source,
                    existingRef = dupResult.ExistingRef
                });
            }

            // Semantic duplicate check (AI)
            if (!dto.IgnoreSemanticWarning)
            {
                var shortDescForCheck = _descEngine.GenerateShortDescription(dto.Noun, dto.Modifier, dto.Attributes);
                var semanticResult = await _dupDetector.SemanticDuplicateCheckAsync(shortDescForCheck, cleanPlant);
                if (semanticResult.IsBlocked)
                {
                    return Conflict(new
                    {
                        status = "DUPLICATED",
                        message = semanticResult.Message,
                        source = "AI_SemanticSimilarity",
                        similarItems = semanticResult.TopMatches
                    });
                }
                else if (semanticResult.IsSuspicious)
                {
                    return Ok(new
                    {
                        warning = true,
                        message = semanticResult.Message,
                        similarItems = semanticResult.TopMatches
                    });
                }
            }

            // Generate descriptions
            var shortDesc = _descEngine.GenerateShortDescription(dto.Noun, dto.Modifier, dto.Attributes);
            var longDesc = _descEngine.GenerateLongDescription(dto.Noun, dto.Modifier, dto.Attributes);

            // Route through lifecycle
            var totalStages = _lifecycleRouter.GetTotalStages(dto.RequestType);
            var nextRole = _lifecycleRouter.GetRoleForStage(dto.RequestType, 2); // Advance past Stage 1

            // Create request record — already past Stage 1 validation
            var request = new ItemRequest
            {
                RequestRefNo = _ticketGen.GenerateTicketRef(_db),
                RequestType = dto.RequestType,
                Noun = dto.Noun.Trim().ToUpper(),
                Modifier = dto.Modifier.Trim().ToUpper(),
                Plant = cleanPlant,
                JsonAttributeValues = JsonSerializer.Serialize(dto.Attributes),
                ShortDescription = shortDesc,
                LongDescription = longDesc,
                CurrentStage = 2, // Auto-advanced past Stage 1
                TotalStages = totalStages,
                ApprovalStatus = "Stage1_Validated",
                DuplicateHash = hash,
                CurrentOwnerRole = nextRole,
                ApprovalLog = $"[{DateTime.UtcNow:o}] Stage 1: Auto-validated. Description generated. No duplicates found for plant {cleanPlant}.",
                CreatedAt = DateTime.UtcNow
            };

            _db.ItemRequests.Add(request);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                id = request.Id,
                status = "Stage1_Validated",
                requestRefNo = request.RequestRefNo,
                shortDescription = request.ShortDescription,
                longDescription = request.LongDescription,
                currentStage = request.CurrentStage,
                totalStages = request.TotalStages,
                currentOwner = request.CurrentOwnerRole,
                plant = request.Plant,
                pipeline = dto.RequestType switch
                {
                    "Single" or "Multiple" => "4-Stage (Requester → Approver → Central Cataloger → Central Approver)",
                    "Modification" or "Plant_Extension" => "3-Stage (Requester → Approver → Central Cataloger)",
                    _ => "4-Stage (Default)"
                }
            });
        }

        /// <summary>
        /// POST /api/requests/bulk
        /// Bulk upload multiple material lines. Each line is processed through the dedup check.
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkUpload([FromBody] BulkUploadDto dto)
        {
            var results = new List<BulkUploadLineResult>();

            for (int i = 0; i < dto.Items.Count; i++)
            {
                var item = dto.Items[i];
                var cleanPlant = string.IsNullOrWhiteSpace(item.Plant) ? "PLT1" : item.Plant.Trim().ToUpper();
                
                var lineResult = new BulkUploadLineResult
                {
                    LineNumber = i + 1,
                    Noun = item.Noun.Trim().ToUpper(),
                    Modifier = item.Modifier.Trim().ToUpper()
                };

                try
                {
                    // Validate schema
                    var schemaExists = await _db.AttributeMaster
                        .AnyAsync(a => a.Noun == item.Noun.Trim().ToUpper() && a.Modifier == item.Modifier.Trim().ToUpper());

                    if (!schemaExists)
                    {
                        lineResult.Status = "Rejected";
                        lineResult.Message = $"No schema found for {item.Noun}/{item.Modifier}.";
                        results.Add(lineResult);
                        continue;
                    }

                    // Dedup check
                    var hash = DuplicateDetector.ComputeHash(item.Noun, item.Modifier, item.Attributes);
                    var dupResult = _dupDetector.CheckForDuplicate(hash, cleanPlant, item.RequestType);

                    if (dupResult.IsDuplicate)
                    {
                        lineResult.Status = "DUPLICATED";
                        lineResult.IsDuplicate = true;
                        lineResult.Message = dupResult.Message;
                        results.Add(lineResult);
                        continue;
                    }

                    // Generate descriptions
                    var shortDesc = _descEngine.GenerateShortDescription(item.Noun, item.Modifier, item.Attributes);
                    var longDesc = _descEngine.GenerateLongDescription(item.Noun, item.Modifier, item.Attributes);

                    var totalStages = _lifecycleRouter.GetTotalStages(item.RequestType);
                    var nextRole = _lifecycleRouter.GetRoleForStage(item.RequestType, 2);

                    var request = new ItemRequest
                    {
                        RequestRefNo = _ticketGen.GenerateTicketRef(_db),
                        RequestType = item.RequestType,
                        Noun = item.Noun.Trim().ToUpper(),
                        Modifier = item.Modifier.Trim().ToUpper(),
                        Plant = cleanPlant,
                        JsonAttributeValues = JsonSerializer.Serialize(item.Attributes),
                        ShortDescription = shortDesc,
                        LongDescription = longDesc,
                        CurrentStage = 2,
                        TotalStages = totalStages,
                        ApprovalStatus = "Stage1_Validated",
                        DuplicateHash = hash,
                        CurrentOwnerRole = nextRole,
                        ApprovalLog = $"[{DateTime.UtcNow:o}] Stage 1: Auto-validated (Bulk Upload Line {i + 1}). No duplicates for plant {cleanPlant}.",
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.ItemRequests.Add(request);
                    await _db.SaveChangesAsync();

                    lineResult.Status = "Stage1_Validated";
                    lineResult.RequestRefNo = request.RequestRefNo;
                    lineResult.Message = $"Created successfully. {shortDesc}";
                }
                catch (Exception ex)
                {
                    lineResult.Status = "Error";
                    lineResult.Message = ex.Message;
                }

                results.Add(lineResult);
            }

            return Ok(new
            {
                totalLines = dto.Items.Count,
                successful = results.Count(r => r.Status == "Stage1_Validated"),
                duplicates = results.Count(r => r.Status == "DUPLICATED"),
                errors = results.Count(r => r.Status == "Rejected" || r.Status == "Error"),
                results
            });
        }

        /// <summary>
        /// GET /api/requests
        /// List all staging requests with optional role filter.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRequests([FromQuery] string? role, [FromQuery] string? status)
        {
            var query = _db.ItemRequests.AsQueryable();

            if (!string.IsNullOrEmpty(role))
                query = query.Where(r => r.CurrentOwnerRole == role);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.ApprovalStatus == status);

            var items = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(items);
        }

        /// <summary>
        /// POST /api/requests/approve
        /// Advance a request through the approval lifecycle.
        /// </summary>
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveRequest([FromBody] ApprovalActionDto dto)
        {
            var request = await _db.ItemRequests.FindAsync(dto.RequestId);
            if (request == null)
                return NotFound($"Request ID {dto.RequestId} not found.");

            if (request.ApprovalStatus == "Approved" || request.ApprovalStatus == "Duplicated" || request.ApprovalStatus == "Rejected")
                return BadRequest($"Request is already in terminal state: {request.ApprovalStatus}");

            if (!string.Equals(request.CurrentOwnerRole, dto.Role, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"Action rejected: Current stage requires '{request.CurrentOwnerRole}' action, but your role is '{dto.Role}'.");
            }

            if (dto.Action == "Reject")
            {
                request.ApprovalStatus = "Rejected";
                request.ApprovalLog += $"\n[{DateTime.UtcNow:o}] Stage {request.CurrentStage}: REJECTED by {dto.Role}. {dto.Comment ?? ""}";
                request.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return Ok(new { status = "Rejected", request });
            }

            // Advance lifecycle
            var advance = _lifecycleRouter.AdvanceStage(request.RequestType, request.CurrentStage);
            request.CurrentStage = advance.NewStage;
            request.CurrentOwnerRole = advance.NewRole;
            request.ApprovalLog += $"\n[{DateTime.UtcNow:o}] Stage {request.CurrentStage}: Approved by {dto.Role}. {dto.Comment ?? ""}";
            request.UpdatedAt = DateTime.UtcNow;

            if (advance.IsComplete)
            {
                request.ApprovalStatus = "Approved";

                // Promote to Golden Master Catalog
                var materialNumber = $"MAT-{request.Id:D6}";
                // Generate AI embedding if configured
                string? embeddingJson = null;
                if (_gemini.IsConfigured)
                {
                    try
                    {
                        var embedding = await _gemini.GenerateEmbeddingAsync(request.ShortDescription);
                        if (embedding != null)
                        {
                            embeddingJson = JsonSerializer.Serialize(embedding);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error generating embedding for promotion: {ex.Message}");
                    }
                }

                var goldenRecord = new GoldenMasterRecord
                {
                    MaterialNumber = materialNumber,
                    SourceRequestRef = request.RequestRefNo,
                    Noun = request.Noun,
                    Modifier = request.Modifier,
                    Plant = request.Plant, // Preserve the plant mapping
                    JsonAttributeValues = request.JsonAttributeValues,
                    ShortDescription = request.ShortDescription,
                    LongDescription = request.LongDescription,
                    DuplicateHash = request.DuplicateHash,
                    EmbeddingVector = embeddingJson,
                    ApprovedAt = DateTime.UtcNow,
                    ApprovedBy = dto.Role
                };

                _db.GoldenMasterCatalog.Add(goldenRecord);
                await _db.SaveChangesAsync();

                // Generate export schema and log
                var exportLog = _exportBridge.LogExport(goldenRecord);

                return Ok(new
                {
                    status = "Approved",
                    message = $"Request approved and promoted to Golden Master Catalog as {materialNumber} for plant {request.Plant}.",
                    materialNumber,
                    exportLogId = exportLog.Id,
                    request
                });
            }
            else
            {
                request.ApprovalStatus = "In_Progress";
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    status = "In_Progress",
                    message = $"Advanced to Stage {advance.NewStage}. Now with: {advance.NewRole}.",
                    request
                });
            }
        }

        /// <summary>
        /// DELETE /api/requests/{id}
        /// Deletes/discards a specific staging request.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var req = await _db.ItemRequests.FindAsync(id);
            if (req == null)
                return NotFound($"Staging request ID {id} not found.");

            _db.ItemRequests.Remove(req);
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Staging request {req.RequestRefNo} successfully deleted." });
        }
    }
}
