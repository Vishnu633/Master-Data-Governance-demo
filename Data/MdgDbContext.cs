using Hofinsoft.Mdg.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;

namespace Hofinsoft.Mdg.Data
{
    public class MdgDbContext : DbContext
    {
        public MdgDbContext(DbContextOptions<MdgDbContext> options) : base(options)
        {
        }

        public DbSet<MaterialTemplate> MaterialTemplates { get; set; }
        public DbSet<StagingItemRequest> StagingItemRequests { get; set; }
        public DbSet<ProductionCatalog> ProductionCatalog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure value converter for MaterialTemplate.RequiredAttributes (List<string> -> JSON string)
            modelBuilder.Entity<MaterialTemplate>()
                .Property(t => t.RequiredAttributes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                );

            // Configure value converter for StagingItemRequest.AttributeValues (Dictionary<string, string> -> JSON string)
            modelBuilder.Entity<StagingItemRequest>()
                .Property(s => s.AttributeValues)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                );

            // Configure value converter for ProductionCatalog.AttributeValues (Dictionary<string, string> -> JSON string)
            modelBuilder.Entity<ProductionCatalog>()
                .Property(p => p.AttributeValues)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                );

            // Seed MaterialTemplates with expanded solutions-focused spare profiles
            modelBuilder.Entity<MaterialTemplate>().HasData(
                new MaterialTemplate
                {
                    Id = 1,
                    Noun = "BEARING",
                    Modifier = "BALL",
                    RequiredAttributes = new List<string> { "Inside_Diameter", "Outside_Diameter", "Material_Grade" }
                },
                new MaterialTemplate
                {
                    Id = 2,
                    Noun = "VALVE",
                    Modifier = "BALL",
                    RequiredAttributes = new List<string> { "Size", "Pressure_Class", "Body_Material", "End_Connection" }
                },
                new MaterialTemplate
                {
                    Id = 3,
                    Noun = "VALVE",
                    Modifier = "GATE",
                    RequiredAttributes = new List<string> { "Size", "Pressure_Class", "Body_Material", "End_Connection" }
                },
                new MaterialTemplate
                {
                    Id = 4,
                    Noun = "MOTOR",
                    Modifier = "INDUCTION",
                    RequiredAttributes = new List<string> { "Power_HP", "Voltage", "Speed_RPM", "Frame_Size" }
                },
                new MaterialTemplate
                {
                    Id = 5,
                    Noun = "GASKET",
                    Modifier = "SPIRAL_WOUND",
                    RequiredAttributes = new List<string> { "Size", "Pressure_Class", "Winding_Material", "Filler_Material" }
                }
            );
        }
    }
}
