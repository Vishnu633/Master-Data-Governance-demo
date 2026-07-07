using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Hofinsoft.Mdg.Models;
using Hofinsoft.Mdg.Services;

namespace Hofinsoft.Mdg.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedGoldenCatalog(NomcatDbContext db, DescriptionEngine descEngine)
        {
            // Seed up to 50 unique records if catalog is empty or has very few items
            if (db.GoldenMasterCatalog.Count() >= 50) return;

            // Clear existing data for a clean, consistent demo environment of exactly 50 records
            db.GoldenMasterCatalog.RemoveRange(db.GoldenMasterCatalog);
            db.ItemRequests.RemoveRange(db.ItemRequests);
            db.ExportLogs.RemoveRange(db.ExportLogs);
            db.SaveChanges();

            var records = new List<GoldenMasterRecord>();
            var requests = new List<ItemRequest>();

            string[] bearingMaterials = { "Steel", "Stainless Steel", "Ceramic", "Chrome Steel", "Bronze" };
            string[] boltMaterials = { "Grade 8.8 Carbon Steel", "Grade 10.9 Alloy Steel", "A2-70 Stainless Steel", "Brass" };
            string[] boltTypes = { "HEX", "STUD", "CARRIAGE", "FLANGE", "SQUARE" };
            string[] plants = { "PLT1", "PLT2", "PLT3" };

            // 1. Generate 25 unique BEARINGS
            for (int i = 1; i <= 25; i++)
            {
                int insideDiam = 10 + i;        // 11mm to 35mm
                int outsideDiam = 30 + (i * 2);  // 32mm to 80mm
                string material = bearingMaterials[i % bearingMaterials.Length];
                string plant = plants[i % plants.Length];

                var attrs = new Dictionary<string, string>
                {
                    { "Inside_Diameter", $"{insideDiam}mm" },
                    { "Outside_Diameter", $"{outsideDiam}mm" },
                    { "Material", material }
                };

                var shortDesc = descEngine.GenerateShortDescription("BEARING", "BALL", attrs);
                var longDesc = descEngine.GenerateLongDescription("BEARING", "BALL", attrs);
                var hash = DuplicateDetector.ComputeHash("BEARING", "BALL", attrs);
                var ticket = $"NMSR/{DateTime.UtcNow.Year:D4}/{DateTime.UtcNow.Month:D2}/{i:D4}";
                var matNumber = $"MAT-{i:D6}";

                // Create the staging request (Approved)
                var req = new ItemRequest
                {
                    Id = i,
                    RequestRefNo = ticket,
                    RequestType = "Single",
                    Noun = "BEARING",
                    Modifier = "BALL",
                    Plant = plant,
                    JsonAttributeValues = JsonSerializer.Serialize(attrs),
                    ShortDescription = shortDesc,
                    LongDescription = longDesc,
                    CurrentStage = 4,
                    TotalStages = 4,
                    ApprovalStatus = "Approved",
                    DuplicateHash = hash,
                    CurrentOwnerRole = "CentralApprover",
                    ApprovalLog = $"[Seeded] Approved and promoted to Golden Catalog.",
                    CreatedAt = DateTime.UtcNow.AddDays(-30 + i),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30 + i)
                };

                // Create the Golden Record
                var gr = new GoldenMasterRecord
                {
                    Id = i,
                    MaterialNumber = matNumber,
                    SourceRequestRef = ticket,
                    Noun = "BEARING",
                    Modifier = "BALL",
                    Plant = plant,
                    JsonAttributeValues = JsonSerializer.Serialize(attrs),
                    ShortDescription = shortDesc,
                    LongDescription = longDesc,
                    DuplicateHash = hash,
                    EmbeddingVector = null, // Embeddings will build dynamically on new approvals
                    ApprovedAt = DateTime.UtcNow.AddDays(-30 + i),
                    ApprovedBy = "CentralApprover"
                };

                requests.Add(req);
                records.Add(gr);
            }

            // 2. Generate 25 unique BOLTS
            for (int i = 1; i <= 25; i++)
            {
                int id = 25 + i;
                string type = boltTypes[i % boltTypes.Length];
                string thread = $"M{6 + (i % 6) * 2}"; // M6, M8, M10, M12, M14, M16
                string length = $"{20 + (i * 2)}mm";  // 22mm to 70mm
                string material = boltMaterials[i % boltMaterials.Length];
                string plant = plants[i % plants.Length];

                var attrs = new Dictionary<string, string>
                {
                    { "Type", type },
                    { "Thread", thread },
                    { "Length", length },
                    { "Material", material }
                };

                var shortDesc = descEngine.GenerateShortDescription("BOLT", "STUD", attrs);
                var longDesc = descEngine.GenerateLongDescription("BOLT", "STUD", attrs);
                var hash = DuplicateDetector.ComputeHash("BOLT", "STUD", attrs);
                var ticket = $"NMSR/{DateTime.UtcNow.Year:D4}/{DateTime.UtcNow.Month:D2}/{id:D4}";
                var matNumber = $"MAT-{id:D6}";

                // Create the staging request (Approved)
                var req = new ItemRequest
                {
                    Id = id,
                    RequestRefNo = ticket,
                    RequestType = "Single",
                    Noun = "BOLT",
                    Modifier = "STUD",
                    Plant = plant,
                    JsonAttributeValues = JsonSerializer.Serialize(attrs),
                    ShortDescription = shortDesc,
                    LongDescription = longDesc,
                    CurrentStage = 4,
                    TotalStages = 4,
                    ApprovalStatus = "Approved",
                    DuplicateHash = hash,
                    CurrentOwnerRole = "CentralApprover",
                    ApprovalLog = $"[Seeded] Approved and promoted to Golden Catalog.",
                    CreatedAt = DateTime.UtcNow.AddDays(-30 + i),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30 + i)
                };

                // Create the Golden Record
                var gr = new GoldenMasterRecord
                {
                    Id = id,
                    MaterialNumber = matNumber,
                    SourceRequestRef = ticket,
                    Noun = "BOLT",
                    Modifier = "STUD",
                    Plant = plant,
                    JsonAttributeValues = JsonSerializer.Serialize(attrs),
                    ShortDescription = shortDesc,
                    LongDescription = longDesc,
                    DuplicateHash = hash,
                    EmbeddingVector = null,
                    ApprovedAt = DateTime.UtcNow.AddDays(-30 + i),
                    ApprovedBy = "CentralApprover"
                };

                requests.Add(req);
                records.Add(gr);
            }

            db.ItemRequests.AddRange(requests);
            db.GoldenMasterCatalog.AddRange(records);
            db.SaveChanges();
        }
    }
}
