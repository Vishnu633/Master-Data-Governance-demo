using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Models;
using Hofinsoft.Mdg.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hofinsoft.Mdg.Controllers
{
    [ApiController]
    [Route("api/staging")]
    public class StagingController : ControllerBase
    {
        private readonly MdgDbContext _context;
        private readonly CamundaWorkflowService _workflowService;

        public StagingController(MdgDbContext context, CamundaWorkflowService workflowService)
        {
            _context = context;
            _workflowService = workflowService;
        }

        // GET: api/staging
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StagingItemRequest>>> GetStagingItems()
        {
            return await _context.StagingItemRequests.ToListAsync();
        }

        // GET: api/staging/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<StagingItemRequest>> GetStagingItem(int id)
        {
            var item = await _context.StagingItemRequests.FindAsync(id);
            if (item == null)
            {
                return NotFound($"Staging item with ID {id} not found.");
            }
            return item;
        }

        // POST: api/staging
        [HttpPost]
        public async Task<ActionResult<StagingItemRequest>> CreateStagingItem(StagingItemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Noun) || string.IsNullOrWhiteSpace(request.Modifier))
            {
                return BadRequest(new { Message = "Noun and Modifier are required." });
            }

            request.Noun = request.Noun.Trim().ToUpper();
            request.Modifier = request.Modifier.Trim().ToUpper();

            // Find matching material template
            var template = await _context.MaterialTemplates
                .FirstOrDefaultAsync(t => t.Noun == request.Noun && t.Modifier == request.Modifier);

            if (template == null)
            {
                return BadRequest(new { Message = $"Governed Material Template for '{request.Noun} {request.Modifier}' does not exist." });
            }

            // Perform validation if status is Pending
            if (request.Status == ApprovalStatus.Pending)
            {
                var validationErrors = ValidateAttributes(template, request.AttributeValues);
                if (validationErrors.Any())
                {
                    return BadRequest(new { Message = "Governance validation failed.", Errors = validationErrors });
                }
            }
            else if (request.Status == ApprovalStatus.Approved)
            {
                return BadRequest(new { Message = "Cannot create an item directly as Approved. Use the approve endpoint." });
            }

            // Calculate Unique ID
            request.UniqueId = CalculateUniqueId(request.Noun, request.Modifier, request.AttributeValues);

            _context.StagingItemRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStagingItem), new { id = request.Id }, request);
        }

        // PUT: api/staging/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStagingItem(int id, StagingItemRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { Message = "ID mismatch." });
            }

            var existingItem = await _context.StagingItemRequests.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound($"Staging item with ID {id} not found.");
            }

            if (existingItem.Status == ApprovalStatus.Approved)
            {
                return BadRequest(new { Message = "Cannot modify a staging item that is already Approved." });
            }

            request.Noun = request.Noun.Trim().ToUpper();
            request.Modifier = request.Modifier.Trim().ToUpper();

            var template = await _context.MaterialTemplates
                .FirstOrDefaultAsync(t => t.Noun == request.Noun && t.Modifier == request.Modifier);

            if (template == null)
            {
                return BadRequest(new { Message = $"Governed Material Template for '{request.Noun} {request.Modifier}' does not exist." });
            }

            // Perform validation if moving to Pending
            if (request.Status == ApprovalStatus.Pending)
            {
                var validationErrors = ValidateAttributes(template, request.AttributeValues);
                if (validationErrors.Any())
                {
                    return BadRequest(new { Message = "Governance validation failed.", Errors = validationErrors });
                }
            }
            else if (request.Status == ApprovalStatus.Approved)
            {
                return BadRequest(new { Message = "Cannot set status directly to Approved. Use the approve endpoint." });
            }

            // Update details
            existingItem.Noun = request.Noun;
            existingItem.Modifier = request.Modifier;
            existingItem.AttributeValues = request.AttributeValues;
            existingItem.Status = request.Status;
            existingItem.UniqueId = CalculateUniqueId(request.Noun, request.Modifier, request.AttributeValues);

            _context.Entry(existingItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(existingItem);
        }

        // POST: api/staging/{id}/approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveStagingItem(int id)
        {
            var result = await _workflowService.ApproveStagingItemWithWorkflowAsync(id);
            if (!result.Success)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result.Message);
                }
                if (result.Message.Contains("already exists") || result.Message.Contains("already been approved"))
                {
                    return Conflict(result.Message);
                }
                return BadRequest(new { Message = result.Message });
            }

            return Ok(new { Message = "Staging item approved successfully (via Camunda workflow mock).", ProductionRecord = result.ProductionRecord });
        }

        private List<string> ValidateAttributes(MaterialTemplate template, Dictionary<string, string> attributeValues)
        {
            var errors = new List<string>();

            // Check for missing required attributes
            foreach (var reqAttr in template.RequiredAttributes)
            {
                if (!attributeValues.TryGetValue(reqAttr, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Missing required attribute: '{reqAttr}'");
                }
            }

            // Check for attributes not defined in the template
            foreach (var attrKey in attributeValues.Keys)
            {
                if (!template.RequiredAttributes.Contains(attrKey))
                {
                    errors.Add($"Attribute '{attrKey}' is not part of the template for '{template.Noun} {template.Modifier}'");
                }
            }

            return errors;
        }

        private string CalculateUniqueId(string noun, string modifier, Dictionary<string, string> attributeValues)
        {
            var sortedPairs = new List<string>();
            foreach (var key in attributeValues.Keys)
            {
                // Ensure key exists and value is trimmed
                var val = attributeValues[key] ?? string.Empty;
                sortedPairs.Add($"{key.Trim().ToUpper()}={val.Trim().ToUpper()}");
            }
            // Sort key-value specifications alphabetically to ensure deterministic hash regardless of input order
            sortedPairs.Sort();
            string rawSpecs = string.Join(";", sortedPairs);

            using (var sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(rawSpecs);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                string hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
                
                // Return Noun-Modifier-First8CharsOfHash
                return $"{noun.Trim().ToUpper()}-{modifier.Trim().ToUpper()}-{hashHex.Substring(0, 8)}";
            }
        }
    }
}
