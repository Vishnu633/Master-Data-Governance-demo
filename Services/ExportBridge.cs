using System;
using System.Text.Json;
using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Models;

namespace Hofinsoft.Mdg.Services
{
    /// <summary>
    /// Export bridge: generates standardized JSON schemas for approved records
    /// and logs downstream transactions.
    /// </summary>
    public class ExportBridge
    {
        private readonly NomcatDbContext _db;

        public ExportBridge(NomcatDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Generates the standardized export schema for a golden master record.
        /// </summary>
        public string GenerateExportSchema(GoldenMasterRecord record)
        {
            var exportObj = new
            {
                schema_version = "1.0",
                material_number = record.MaterialNumber,
                source_request = record.SourceRequestRef,
                classification = new
                {
                    noun = record.Noun,
                    modifier = record.Modifier
                },
                attributes = JsonSerializer.Deserialize<object>(record.JsonAttributeValues),
                descriptions = new
                {
                    @short = record.ShortDescription,
                    @long = record.LongDescription
                },
                metadata = new
                {
                    approved_at = record.ApprovedAt.ToString("o"),
                    approved_by = record.ApprovedBy,
                    duplicate_hash = record.DuplicateHash
                }
            };

            return JsonSerializer.Serialize(exportObj, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Creates an export log entry after successful promotion to golden catalog.
        /// </summary>
        public ExportLog LogExport(GoldenMasterRecord record)
        {
            var schema = GenerateExportSchema(record);

            // Update the record's export schema
            record.ExportSchema = schema;
            _db.GoldenMasterCatalog.Update(record);

            var log = new ExportLog
            {
                RequestRefNo = record.SourceRequestRef,
                MaterialNumber = record.MaterialNumber,
                ExportPayload = schema,
                Status = "Exported",
                ExportedAt = DateTime.UtcNow
            };

            _db.ExportLogs.Add(log);
            _db.SaveChanges();

            return log;
        }
    }
}
