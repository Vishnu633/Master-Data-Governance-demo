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
