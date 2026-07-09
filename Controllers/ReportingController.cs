using Hofinsoft.Mdg.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hofinsoft.Mdg.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportingController : ControllerBase
    {
        private readonly NomcatDbContext _db;

        public ReportingController(NomcatDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/reporting/staging-export
        /// Returns a CSV-formatted download of all staging items for Excel import.
        /// </summary>
        [HttpGet("staging-export")]
        public async Task<IActionResult> ExportStagingToCsv()
        {
            var items = await _db.ItemRequests
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Request_Ref_No,Request_Type,Noun,Modifier,Short_Description,Current_Stage,Total_Stages,Approval_Status,Current_Owner,Created_At");

            foreach (var item in items)
            {
                sb.AppendLine($"\"{item.RequestRefNo}\",\"{item.RequestType}\",\"{item.Noun}\",\"{item.Modifier}\",\"{item.ShortDescription}\",{item.CurrentStage},{item.TotalStages},\"{item.ApprovalStatus}\",\"{item.CurrentOwnerRole}\",\"{item.CreatedAt:o}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "nomcat_staging_report.csv");
        }

        /// <summary>
        /// GET /api/reporting/catalog-export
        /// Returns a CSV-formatted download of the Golden Master Catalog.
        /// </summary>
        [HttpGet("catalog-export")]
        public async Task<IActionResult> ExportCatalogToCsv()
        {
            var items = await _db.GoldenMasterCatalog
                .OrderBy(c => c.MaterialNumber)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Material_Number,Noun,Modifier,Plant,Short_Description,Long_Description,Source_Request_Ref,Approved_By,Approved_At");

            foreach (var item in items)
            {
                sb.AppendLine($"\"{item.MaterialNumber}\",\"{item.Noun}\",\"{item.Modifier}\",\"{item.Plant}\",\"{item.ShortDescription}\",\"{item.LongDescription}\",\"{item.SourceRequestRef}\",\"{item.ApprovedBy}\",\"{item.ApprovedAt:o}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "nomcat_golden_catalog_report.csv");
        }

        /// <summary>
        /// GET /api/reporting/staging-export-filtered
        /// Returns a filtered CSV-formatted staging download.
        /// </summary>
        [HttpGet("staging-export-filtered")]
        public async Task<IActionResult> ExportStagingFilteredToCsv([FromQuery] string? plant, [FromQuery] string? noun, [FromQuery] string? status)
        {
            var query = _db.ItemRequests.AsQueryable();

            if (!string.IsNullOrEmpty(plant) && !string.Equals(plant, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Plant == plant.Trim().ToUpper());
            }

            if (!string.IsNullOrEmpty(noun) && !string.Equals(noun, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Noun == noun.Trim().ToUpper());
            }

            if (!string.IsNullOrEmpty(status) && !string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.ApprovalStatus == status.Trim());
            }

            var items = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Request_Ref_No,Request_Type,Noun,Modifier,Short_Description,Current_Stage,Total_Stages,Approval_Status,Current_Owner,Created_At");

            foreach (var item in items)
            {
                sb.AppendLine($"\"{item.RequestRefNo}\",\"{item.RequestType}\",\"{item.Noun}\",\"{item.Modifier}\",\"{item.ShortDescription}\",{item.CurrentStage},{item.TotalStages},\"{item.ApprovalStatus}\",\"{item.CurrentOwnerRole}\",\"{item.CreatedAt:o}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "nomcat_staging_report_filtered.csv");
        }

        /// <summary>
        /// GET /api/reporting/summary
        /// Returns summary statistics for the dashboard.
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var totalRequests = await _db.ItemRequests.CountAsync();
            var pending = await _db.ItemRequests.CountAsync(r => r.ApprovalStatus == "Stage1_Validated" || r.ApprovalStatus == "In_Progress");
            var approved = await _db.ItemRequests.CountAsync(r => r.ApprovalStatus == "Approved");
            var duplicated = await _db.ItemRequests.CountAsync(r => r.ApprovalStatus == "Duplicated");
            var rejected = await _db.ItemRequests.CountAsync(r => r.ApprovalStatus == "Rejected");
            var goldenRecords = await _db.GoldenMasterCatalog.CountAsync();
            var exports = await _db.ExportLogs.CountAsync();

            return Ok(new
            {
                totalRequests,
                pending,
                approved,
                duplicated,
                rejected,
                goldenRecords,
                exports
            });
        }
    }
}
