using System;
using System.Collections.Generic;

namespace Hofinsoft.Mdg.Models
{
    /// <summary>
    /// Clean, active manufacturing master inventory — the Golden Record.
    /// </summary>
    public class GoldenMasterRecord
    {
        public int Id { get; set; }

        /// <summary>Material number in golden catalog</summary>
        public string MaterialNumber { get; set; } = string.Empty;

        /// <summary>Source request ticket reference</summary>
        public string SourceRequestRef { get; set; } = string.Empty;

        public string Noun { get; set; } = string.Empty;
        public string Modifier { get; set; } = string.Empty;

        /// <summary>Plant Code where the material is assigned (e.g. PLT1, PLT2)</summary>
        public string Plant { get; set; } = "PLT1";

        /// <summary>JSON-serialized final attribute values</summary>
        public string JsonAttributeValues { get; set; } = "{}";

        public string ShortDescription { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;

        /// <summary>Deterministic hash for dedup matching</summary>
        public string DuplicateHash { get; set; } = string.Empty;

        /// <summary>JSON export schema for downstream systems</summary>
        public string ExportSchema { get; set; } = "{}";

        /// <summary>JSON-serialized float[] embedding vector for semantic search (nullable)</summary>
        public string? EmbeddingVector { get; set; }

        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
        public string ApprovedBy { get; set; } = "System";
    }
}
