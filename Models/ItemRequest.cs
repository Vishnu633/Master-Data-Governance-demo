using System;
using System.Collections.Generic;

namespace Hofinsoft.Mdg.Models
{
    /// <summary>
    /// Staging table holding material requests through the approval lifecycle.
    /// </summary>
    public class ItemRequest
    {
        public int Id { get; set; }

        /// <summary>Auto-generated ticket: NMSR/2026/MM/XXXX</summary>
        public string RequestRefNo { get; set; } = string.Empty;

        /// <summary>Single, Multiple, Modification, Plant_Extension</summary>
        public string RequestType { get; set; } = "Single";

        public string Noun { get; set; } = string.Empty;
        public string Modifier { get; set; } = string.Empty;

        /// <summary>Plant Code where the material is assigned (e.g. PLT1, PLT2)</summary>
        public string Plant { get; set; } = "PLT1";

        /// <summary>JSON-serialized attribute key-value pairs</summary>
        public string JsonAttributeValues { get; set; } = "{}";

        /// <summary>Generated short description: "BEARING, BALL: 20MM ID, 32MM OD"</summary>
        public string ShortDescription { get; set; } = string.Empty;

        /// <summary>Generated long description with full attribute detail</summary>
        public string LongDescription { get; set; } = string.Empty;

        /// <summary>Current stage in the approval pipeline (1-4)</summary>
        public int CurrentStage { get; set; } = 1;

        /// <summary>Total stages in this request's pipeline (3 or 4)</summary>
        public int TotalStages { get; set; } = 4;

        /// <summary>Pending, Stage1_Validated, In_Progress, Approved, Rejected, Duplicated</summary>
        public string ApprovalStatus { get; set; } = "Pending";

        /// <summary>Deterministic hash of noun+modifier+attributes for dedup</summary>
        public string DuplicateHash { get; set; } = string.Empty;

        /// <summary>Simulated current role owner: Requester, Approver, Cataloger, CentralApprover</summary>
        public string CurrentOwnerRole { get; set; } = "Requester";

        /// <summary>Comma-separated approval log trail</summary>
        public string ApprovalLog { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
