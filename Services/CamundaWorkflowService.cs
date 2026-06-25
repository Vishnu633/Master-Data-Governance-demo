using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hofinsoft.Mdg.Services
{
    public class CamundaWorkflowService
    {
        private readonly MdgDbContext _context;

        public CamundaWorkflowService(MdgDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, ProductionCatalog? ProductionRecord)> ApproveStagingItemWithWorkflowAsync(int stagingItemId)
        {
            // 1. Fetch the staging item
            var stagingItem = await _context.StagingItemRequests.FindAsync(stagingItemId);
            if (stagingItem == null)
            {
                return (false, $"Staging item with ID {stagingItemId} not found.", null);
            }

            if (stagingItem.Status == ApprovalStatus.Approved)
            {
                return (false, "This staging item has already been approved.", null);
            }

            // 2. Fetch the corresponding template
            var template = await _context.MaterialTemplates
                .FirstOrDefaultAsync(t => t.Noun == stagingItem.Noun && t.Modifier == stagingItem.Modifier);

            if (template == null)
            {
                return (false, $"Governed Material Template for '{stagingItem.Noun} {stagingItem.Modifier}' no longer exists.", null);
            }

            // 3. Governance Validation
            var validationErrors = ValidateAttributes(template, stagingItem.AttributeValues);
            if (validationErrors.Any())
            {
                return (false, "Cannot approve. Governance validation failed.", null);
            }

            // 4. Generate Nomenclature Description if missing
            if (string.IsNullOrWhiteSpace(stagingItem.Description))
            {
                string attr1Key = template.RequiredAttributes.ElementAtOrDefault(0) ?? "";
                string attr2Key = template.RequiredAttributes.ElementAtOrDefault(1) ?? "";
                string attr3Key = template.RequiredAttributes.ElementAtOrDefault(2) ?? "";

                string val1 = stagingItem.AttributeValues.GetValueOrDefault(attr1Key) ?? "";
                string val2 = stagingItem.AttributeValues.GetValueOrDefault(attr2Key) ?? "";
                string val3 = stagingItem.AttributeValues.GetValueOrDefault(attr3Key) ?? "";

                stagingItem.Description = $"{stagingItem.Noun}, {stagingItem.Modifier}: {val1} ID, {val2} OD, {val3}";
            }

            // 5. Calculate deterministic Unique ID
            string uniqueId = CalculateUniqueId(stagingItem.Noun, stagingItem.Modifier, stagingItem.AttributeValues);
            stagingItem.UniqueId = uniqueId;

            // 6. Check for duplicate in Production Catalog
            var alreadyInProduction = await _context.ProductionCatalog.AnyAsync(p => p.UniqueId == uniqueId);
            if (alreadyInProduction)
            {
                return (false, $"A golden record with Unique ID '{uniqueId}' already exists in the Production Catalog.", null);
            }

            // 7. Mock Camunda Workflow Execution
            string processInstanceId = $"PR-{Guid.NewGuid().ToString().Substring(0, 8)}-{Guid.NewGuid().ToString().Substring(0, 4)}";
            Console.WriteLine($"[CAMUNDA WORKFLOW] Triggered by Staging ID: {stagingItemId}");
            Console.WriteLine($"[CAMUNDA WORKFLOW] Process Instance Created: {processInstanceId}");
            Console.WriteLine($"[CAMUNDA WORKFLOW] Task Definition Key: UserTask_ApproveMaterial");
            Console.WriteLine($"[CAMUNDA WORKFLOW] Task Status: COMPLETED");
            Console.WriteLine($"[CAMUNDA WORKFLOW] Decision Gate: APPROVED. Promoting record with Unique ID '{uniqueId}'");

            // 8. Add to Production Catalog (Golden Master Record)
            var goldenRecord = new ProductionCatalog
            {
                UniqueId = uniqueId,
                Noun = stagingItem.Noun,
                Modifier = stagingItem.Modifier,
                AttributeValues = stagingItem.AttributeValues,
                Description = stagingItem.Description,
                ApprovedAt = DateTime.UtcNow
            };

            _context.ProductionCatalog.Add(goldenRecord);

            // Update Staging Status to Approved
            stagingItem.Status = ApprovalStatus.Approved;

            await _context.SaveChangesAsync();

            return (true, "Staging item approved successfully.", goldenRecord);
        }

        private List<string> ValidateAttributes(MaterialTemplate template, Dictionary<string, string> attributeValues)
        {
            var errors = new List<string>();
            foreach (var reqAttr in template.RequiredAttributes)
            {
                if (!attributeValues.TryGetValue(reqAttr, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Missing required attribute: '{reqAttr}'");
                }
            }
            foreach (var attrKey in attributeValues.Keys)
            {
                if (!template.RequiredAttributes.Contains(attrKey))
                {
                    errors.Add($"Attribute '{attrKey}' is not part of the template.");
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
