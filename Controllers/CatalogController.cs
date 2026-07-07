using Hofinsoft.Mdg.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Hofinsoft.Mdg.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly NomcatDbContext _db;

        public CatalogController(NomcatDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/catalog
        /// Returns all Golden Master Catalog records.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCatalog()
        {
            var records = await _db.GoldenMasterCatalog
                .OrderByDescending(g => g.ApprovedAt)
                .ToListAsync();
            return Ok(records);
        }

        /// <summary>
        /// GET /api/catalog/{id}/export
        /// Returns the export schema for a specific golden record.
        /// </summary>
        [HttpGet("{id}/export")]
        public async Task<IActionResult> GetExportSchema(int id)
        {
            var record = await _db.GoldenMasterCatalog.FindAsync(id);
            if (record == null)
                return NotFound($"Golden record ID {id} not found.");

            return Ok(new
            {
                materialNumber = record.MaterialNumber,
                exportSchema = record.ExportSchema
            });
        }

        /// <summary>
        /// GET /api/catalog/exports
        /// Returns all export log entries.
        /// </summary>
        [HttpGet("exports")]
        public async Task<IActionResult> GetExportLogs()
        {
            var logs = await _db.ExportLogs
                .OrderByDescending(l => l.ExportedAt)
                .ToListAsync();
            return Ok(logs);
        }
    }
}
