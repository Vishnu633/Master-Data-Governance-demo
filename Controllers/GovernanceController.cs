using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Models;
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
    [Route("api/governance")]
    public class GovernanceController : ControllerBase
    {
        private readonly MdgDbContext _context;

        public GovernanceController(MdgDbContext context)
        {
            _context = context;
        }

        // POST: api/governance/request
        [HttpPost("request")]
        public async Task<ActionResult<StagingItemRequest>> SubmitGovernanceRequest(StagingItemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Noun) || string.IsNullOrWhiteSpace(request.Modifier))
            {
                return BadRequest(new { Message = "Noun and Modifier are required." });
            }

            request.Noun = request.Noun.Trim().ToUpper();
            request.Modifier = request.Modifier.Trim().ToUpper();

            // 1. Template Validation
            var template = await _context.MaterialTemplates
                .FirstOrDefaultAsync(t => t.Noun == request.Noun && t.Modifier == request.Modifier);

            if (template == null)
            {
                return BadRequest(new { Message = $"Governed Material Template for '{request.Noun} {request.Modifier}' does not exist." });
            }

            // Perform full attribute validation (required for Pending/Governance submissions)
            var validationErrors = ValidateAttributes(template, request.AttributeValues);
            if (validationErrors.Any())
            {
                return BadRequest(new { Message = "Governance validation failed.", Errors = validationErrors });
            }

            // 2. Nomenclature Generator
            // Formatted as: [Noun], [Modifier]: [Attr1_Value] ID, [Attr2_Value] OD, [Attr3_Value]
            string attr1Key = template.RequiredAttributes.ElementAtOrDefault(0) ?? "";
            string attr2Key = template.RequiredAttributes.ElementAtOrDefault(1) ?? "";
            string attr3Key = template.RequiredAttributes.ElementAtOrDefault(2) ?? "";

            string val1 = request.AttributeValues.GetValueOrDefault(attr1Key) ?? "";
            string val2 = request.AttributeValues.GetValueOrDefault(attr2Key) ?? "";
            string val3 = request.AttributeValues.GetValueOrDefault(attr3Key) ?? "";

            request.Description = $"{request.Noun}, {request.Modifier}: {val1} ID, {val2} OD, {val3}";

            // 3. Similarity / De-duplication Engine
            // Calculate deterministic Unique ID based on noun, modifier, and attributes
            string uniqueId = CalculateUniqueId(request.Noun, request.Modifier, request.AttributeValues);
            request.UniqueId = uniqueId;

            // Check if exact record already exists in the Production Catalog
            var existsInProduction = await _context.ProductionCatalog
                .AnyAsync(p => p.UniqueId == uniqueId);

            if (existsInProduction)
            {
                return BadRequest(new { Message = "Duplicate entry blocked by governance logic." });
            }

            // Save to Staging with status Pending
            request.Status = ApprovalStatus.Pending;

            _context.StagingItemRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStagingItem", "Staging", new { id = request.Id }, request);
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
                var val = attributeValues[key] ?? string.Empty;
                sortedPairs.Add($"{key.Trim().ToUpper()}={val.Trim().ToUpper()}");
            }
            sortedPairs.Sort();
            string rawSpecs = string.Join(";", sortedPairs);

            using (var sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(rawSpecs);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                string hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
                return $"{noun.Trim().ToUpper()}-{modifier.Trim().ToUpper()}-{hashHex.Substring(0, 8)}";
            }
        }
    }
}
