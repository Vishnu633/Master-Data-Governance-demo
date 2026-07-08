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
            // Seed up to 100 unique records if catalog is empty or has very few items
            if (db.GoldenMasterCatalog.Count() >= 100) return;

            // Clear existing data for a clean, consistent demo environment of exactly 100 records
            db.GoldenMasterCatalog.RemoveRange(db.GoldenMasterCatalog);
            db.ItemRequests.RemoveRange(db.ItemRequests);
            db.ExportLogs.RemoveRange(db.ExportLogs);
            db.SaveChanges();

            var records = new List<GoldenMasterRecord>();
            var requests = new List<ItemRequest>();

            string[] plants = { "PLT1", "PLT2", "PLT3" };

            // Pool arrays for seeding
            string[] bearingMaterials = { "Steel", "Stainless Steel", "Ceramic", "Chrome Steel", "Bronze" };
            
            string[] boltMaterials = { "Grade 8.8 Carbon Steel", "Grade 10.9 Alloy Steel", "A2-70 Stainless Steel", "Brass" };
            string[] boltTypes = { "HEX", "STUD", "CARRIAGE", "FLANGE", "SQUARE" };

            string[] gasketTypes = { "RING", "SPIRAL WOUND", "FULL FACE", "ENVELOPE", "RTJ" };
            string[] gasketSizes = { "1/2 in", "1 in", "2 in", "3 in", "4 in" };
            string[] gasketRatings = { "150#", "300#", "600#", "900#", "1500#" };
            string[] gasketThicknesses = { "1.5mm", "3.0mm", "4.5mm" };
            string[] gasketMaterials = { "TEFLON", "GRAPHITE", "NON-ASBESTOS", "AISI 316" };

            string[] valveTypes = { "FULL PORT", "REDUCED PORT", "3-WAY", "V-PORT" };
            string[] valveSizes = { "1/2 in", "1 in", "2 in", "3 in", "4 in" };
            string[] valveRatings = { "Class 150", "Class 300", "Class 600", "Class 900" };
            string[] valveMaterials = { "CF8M STAINLESS STEEL", "A105 CARBON STEEL", "BRONZE", "MONEL" };
            string[] valveConnections = { "FLANGED", "THREADED NPT", "SOCKET WELD", "BUTT WELD" };

            string[] cableVoltages = { "600V", "1KV", "10KV", "33KV" };
            string[] cableCores = { "1C", "2C", "3C", "4C", "12C" };
            string[] cableSizes = { "1.5 SQMM", "2.5 SQMM", "4 SQMM", "10 SQMM", "16 SQMM", "95 SQMM" };
            string[] cableInsulations = { "XLPE", "PVC", "EPR" };
            string[] cableMaterials = { "COPPER", "ALUMINUM" };

            int idCounter = 1;

            // 1. Generate 20 unique BEARINGS
            for (int i = 1; i <= 20; i++)
            {
                int insideDiam = 10 + i;        // 11mm to 30mm
                int outsideDiam = 30 + (i * 2);  // 32mm to 70mm
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
                var ticket = $"NMSR/{DateTime.UtcNow.Year:D4}/{DateTime.UtcNow.Month:D2}/{idCounter:D4}";
                var matNumber = $"MAT-{idCounter:D6}";

                requests.Add(new ItemRequest
                {
                    Id = idCounter,
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
                });

                records.Add(new GoldenMasterRecord
                {
                    Id = idCounter,
                    MaterialNumber = matNumber,
                    SourceRequestRef = ticket,
                    Noun = "BEARING",
                    Modifier = "BALL",
                    Plant = plant,
                    JsonAttributeValues = JsonSerializer.Serialize(attrs),
                    ShortDescription = shortDesc,
                    LongDescription = longDesc,
                    DuplicateHash = hash,
                    EmbeddingVector = null,
                    ApprovedAt = DateTime.UtcNow.AddDays(-30 + i),
                    ApprovedBy = "CentralApprover"
                });

                idCounter++;
            }

            // 2. Generate 20 unique BOLTS
            for (int i = 1; i <= 20; i++)
            {
                string type = boltTypes[i % boltTypes.Length];
                string thread = $"M{6 + (i % 6) * 2}"; // M6, M8, M10, M12, M14, M16
                string length = $"{20 + (i * 2)}mm";  // 22mm to 60mm
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
                var ticket = $"NMSR/{DateTime.UtcNow.Year:D4}/{DateTime.UtcNow.Month:D2}/{idCounter:D4}";
                var matNumber = $"MAT-{idCounter:D6}";

                requests.Add(new ItemRequest
                {
                    Id = idCounter,
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
                });

                records.Add(new GoldenMasterRecord
                {
                    Id = idCounter,
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
                });

                idCounter++;
            }

            // 3. Generate 20 unique GASKETS
            for (int i = 1; i <= 20; i++)
            {
                string type = gasketTypes[i % gasketTypes.Length];
                string size = gasketSizes[i % gasketSizes.Length];
                string rating = gasketRatings[i % gasketRatings.Length];
                string thickness = gasketThicknesses[i % gasketThicknesses.Length];
                string material = gasketMaterials[i % gasketMaterials.Length];
                string plant = plants[i % plants.Length];

                var attrs = new Dictionary<string, string>
                {
                    { "Type", type },
                    { "Size", size },
                    { "Rating", rating },
                    { "Thickness", thickness },
                    { "Material", material }
                };

                var shortDesc = descEngine.GenerateShortDescription("GASKET", "FLANGE", attrs);
                var longDesc = descEngine.GenerateLongDescription("GASKET", "FLANGE", attrs);
                var hash = DuplicateDetector.ComputeHash("GASKET", "FLANGE", attrs);
                var ticket = $"NMSR/{DateTime.UtcNow.Year:D4}/{DateTime.UtcNow.Month:D2}/{idCounter:D4}";
                var matNumber = $"MAT-{idCounter:D6}";

                requests.Add(new ItemRequest
                {
                    Id = idCounter,
                    RequestRefNo = ticket,
                    RequestType = "Single",
                    Noun = "GASKET",
                    Modifier = "FLANGE",
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
                });

                records.Add(new GoldenMasterRecord
                {
                    Id = idCounter,
                    MaterialNumber = matNumber,
                    SourceRequestRef = ticket,
                    Noun = "GASKET",
                    Modifier = "FLANGE",
                    Plant = plant,
                    JsonAttributeValues = JsonSerializer.Serialize(attrs),
                    ShortDescription = shortDesc,
                    LongDescription = longDesc,
                    DuplicateHash = hash,
                    EmbeddingVector = null,
                    ApprovedAt = DateTime.UtcNow.AddDays(-30 + i),
                    ApprovedBy = "CentralApprover"
                });

                idCounter++;
            }

            // 4. Generate 20 unique VALVES
            for (int i = 1; i <= 20; i++)
            {
                string type = valveTypes[i % valveTypes.Length];
                string size = valveSizes[i % valveSizes.Length];
                string rating = valveRatings[i % valveRatings.Length];
                string material = valveMaterials[i % valveMaterials.Length];
                string conn = valveConnections[i % valveConnections.Length];
                string plant = plants[i % plants.Length];

                var attrs = new Dictionary<string, string>
                {
                    { "Type", type },
                    { "Size", size },
                    { "Pressure_Rating", rating },
                    { "Body_Material", material },
                    { "Connection", conn }
                };

                var shortDesc = descEngine.GenerateShortDescription("VALVE", "BALL", attrs);
                var longDesc = descEngine.GenerateLongDescription("VALVE", "BALL", attrs);
                var hash = DuplicateDetector.ComputeHash("VALVE", "BALL", attrs);
                var ticket = $"NMSR/{DateTime.UtcNow.Year:D4}/{DateTime.UtcNow.Month:D2}/{idCounter:D4}";
                var matNumber = $"MAT-{idCounter:D6}";

                requests.Add(new ItemRequest
                {
                    Id = idCounter,
                    RequestRefNo = ticket,
                    RequestType = "Single",
                    Noun = "VALVE",
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
                });

                records.Add(new GoldenMasterRecord
                {
                    Id = idCounter,
                    MaterialNumber = matNumber,
                    SourceRequestRef = ticket,
                    Noun = "VALVE",
                    Modifier = "BALL",
                    Plant = plant,
                    JsonAttributeValues = JsonSerializer.Serialize(attrs),
                    ShortDescription = shortDesc,
                    LongDescription = longDesc,
                    DuplicateHash = hash,
                    EmbeddingVector = null,
                    ApprovedAt = DateTime.UtcNow.AddDays(-30 + i),
                    ApprovedBy = "CentralApprover"
                });

                idCounter++;
            }

            // 5. Generate 20 unique CABLES
            for (int i = 1; i <= 20; i++)
            {
                string voltage = cableVoltages[i % cableVoltages.Length];
                string cores = cableCores[i % cableCores.Length];
                string size = cableSizes[i % cableSizes.Length];
                string insulation = cableInsulations[i % cableInsulations.Length];
                string material = cableMaterials[i % cableMaterials.Length];
                string plant = plants[i % plants.Length];

                var attrs = new Dictionary<string, string>
                {
                    { "Voltage", voltage },
                    { "Cores", cores },
                    { "Size", size },
                    { "Insulation", insulation },
                    { "Conductor_Material", material }
                };

                var shortDesc = descEngine.GenerateShortDescription("CABLE", "ELECTRICAL", attrs);
                var longDesc = descEngine.GenerateLongDescription("CABLE", "ELECTRICAL", attrs);
                var hash = DuplicateDetector.ComputeHash("CABLE", "ELECTRICAL", attrs);
                var ticket = $"NMSR/{DateTime.UtcNow.Year:D4}/{DateTime.UtcNow.Month:D2}/{idCounter:D4}";
                var matNumber = $"MAT-{idCounter:D6}";

                requests.Add(new ItemRequest
                {
                    Id = idCounter,
                    RequestRefNo = ticket,
                    RequestType = "Single",
                    Noun = "CABLE",
                    Modifier = "ELECTRICAL",
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
                });

                records.Add(new GoldenMasterRecord
                {
                    Id = idCounter,
                    MaterialNumber = matNumber,
                    SourceRequestRef = ticket,
                    Noun = "CABLE",
                    Modifier = "ELECTRICAL",
                    Plant = plant,
                    JsonAttributeValues = JsonSerializer.Serialize(attrs),
                    ShortDescription = shortDesc,
                    LongDescription = longDesc,
                    DuplicateHash = hash,
                    EmbeddingVector = null,
                    ApprovedAt = DateTime.UtcNow.AddDays(-30 + i),
                    ApprovedBy = "CentralApprover"
                });

                idCounter++;
            }

            db.ItemRequests.AddRange(requests);
            db.GoldenMasterCatalog.AddRange(records);
            db.SaveChanges();
        }
    }
}
