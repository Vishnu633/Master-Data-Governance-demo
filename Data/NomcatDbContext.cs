using Hofinsoft.Mdg.Models;
using Microsoft.EntityFrameworkCore;

namespace Hofinsoft.Mdg.Data
{
    public class NomcatDbContext : DbContext
    {
        public NomcatDbContext(DbContextOptions<NomcatDbContext> options) : base(options) { }

        public DbSet<AttributeMaster> AttributeMaster { get; set; }
        public DbSet<ItemRequest> ItemRequests { get; set; }
        public DbSet<GoldenMasterRecord> GoldenMasterCatalog { get; set; }
        public DbSet<ExportLog> ExportLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed BEARING BALL attributes
            modelBuilder.Entity<AttributeMaster>().HasData(
                new AttributeMaster { Id = 1, Noun = "BEARING", Modifier = "BALL", AttributeName = "Inside_Diameter", MandatoryIndicator = "Y", SortOrder = 1 },
                new AttributeMaster { Id = 2, Noun = "BEARING", Modifier = "BALL", AttributeName = "Outside_Diameter", MandatoryIndicator = "Y", SortOrder = 2 },
                new AttributeMaster { Id = 3, Noun = "BEARING", Modifier = "BALL", AttributeName = "Material", MandatoryIndicator = "Y", SortOrder = 3 }
            );

            // Seed BOLT STUD attributes
            modelBuilder.Entity<AttributeMaster>().HasData(
                new AttributeMaster { Id = 4, Noun = "BOLT", Modifier = "STUD", AttributeName = "Type", MandatoryIndicator = "Y", SortOrder = 1 },
                new AttributeMaster { Id = 5, Noun = "BOLT", Modifier = "STUD", AttributeName = "Thread", MandatoryIndicator = "Y", SortOrder = 2 },
                new AttributeMaster { Id = 6, Noun = "BOLT", Modifier = "STUD", AttributeName = "Length", MandatoryIndicator = "Y", SortOrder = 3 },
                new AttributeMaster { Id = 7, Noun = "BOLT", Modifier = "STUD", AttributeName = "Material", MandatoryIndicator = "Y", SortOrder = 4 }
            );

            // Seed GASKET FLANGE attributes
            modelBuilder.Entity<AttributeMaster>().HasData(
                new AttributeMaster { Id = 8, Noun = "GASKET", Modifier = "FLANGE", AttributeName = "Type", MandatoryIndicator = "Y", SortOrder = 1 },
                new AttributeMaster { Id = 9, Noun = "GASKET", Modifier = "FLANGE", AttributeName = "Size", MandatoryIndicator = "Y", SortOrder = 2 },
                new AttributeMaster { Id = 10, Noun = "GASKET", Modifier = "FLANGE", AttributeName = "Rating", MandatoryIndicator = "Y", SortOrder = 3 },
                new AttributeMaster { Id = 11, Noun = "GASKET", Modifier = "FLANGE", AttributeName = "Thickness", MandatoryIndicator = "Y", SortOrder = 4 },
                new AttributeMaster { Id = 12, Noun = "GASKET", Modifier = "FLANGE", AttributeName = "Material", MandatoryIndicator = "Y", SortOrder = 5 }
            );

            // Seed VALVE BALL attributes
            modelBuilder.Entity<AttributeMaster>().HasData(
                new AttributeMaster { Id = 13, Noun = "VALVE", Modifier = "BALL", AttributeName = "Type", MandatoryIndicator = "Y", SortOrder = 1 },
                new AttributeMaster { Id = 14, Noun = "VALVE", Modifier = "BALL", AttributeName = "Size", MandatoryIndicator = "Y", SortOrder = 2 },
                new AttributeMaster { Id = 15, Noun = "VALVE", Modifier = "BALL", AttributeName = "Pressure_Rating", MandatoryIndicator = "Y", SortOrder = 3 },
                new AttributeMaster { Id = 16, Noun = "VALVE", Modifier = "BALL", AttributeName = "Body_Material", MandatoryIndicator = "Y", SortOrder = 4 },
                new AttributeMaster { Id = 17, Noun = "VALVE", Modifier = "BALL", AttributeName = "Connection", MandatoryIndicator = "Y", SortOrder = 5 }
            );

            // Seed CABLE ELECTRICAL attributes
            modelBuilder.Entity<AttributeMaster>().HasData(
                new AttributeMaster { Id = 18, Noun = "CABLE", Modifier = "ELECTRICAL", AttributeName = "Voltage", MandatoryIndicator = "Y", SortOrder = 1 },
                new AttributeMaster { Id = 19, Noun = "CABLE", Modifier = "ELECTRICAL", AttributeName = "Cores", MandatoryIndicator = "Y", SortOrder = 2 },
                new AttributeMaster { Id = 20, Noun = "CABLE", Modifier = "ELECTRICAL", AttributeName = "Size", MandatoryIndicator = "Y", SortOrder = 3 },
                new AttributeMaster { Id = 21, Noun = "CABLE", Modifier = "ELECTRICAL", AttributeName = "Insulation", MandatoryIndicator = "Y", SortOrder = 4 },
                new AttributeMaster { Id = 22, Noun = "CABLE", Modifier = "ELECTRICAL", AttributeName = "Conductor_Material", MandatoryIndicator = "Y", SortOrder = 5 }
            );

            // Configure indexes
            modelBuilder.Entity<AttributeMaster>()
                .HasIndex(a => new { a.Noun, a.Modifier });

            modelBuilder.Entity<ItemRequest>()
                .HasIndex(r => r.RequestRefNo).IsUnique();

            // Compound index for staging per-plant duplication check
            modelBuilder.Entity<ItemRequest>()
                .HasIndex(r => new { r.DuplicateHash, r.Plant });

            // Compound index for golden master catalog per-plant duplication check
            modelBuilder.Entity<GoldenMasterRecord>()
                .HasIndex(g => new { g.DuplicateHash, g.Plant });

            modelBuilder.Entity<GoldenMasterRecord>()
                .HasIndex(g => g.MaterialNumber).IsUnique();
        }
    }
}
