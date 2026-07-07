using System;

namespace Hofinsoft.Mdg.Models
{
    /// <summary>
    /// Log tracking downstream export transactions after approval.
    /// </summary>
    public class ExportLog
    {
        public int Id { get; set; }
        public string RequestRefNo { get; set; } = string.Empty;
        public string MaterialNumber { get; set; } = string.Empty;
        public string ExportPayload { get; set; } = "{}";
        public string Status { get; set; } = "Exported";
        public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    }
}
