using System.Collections.Generic;

namespace Hofinsoft.Mdg.Models.Dto
{
    /// <summary>DTO for submitting a new material request.</summary>
    public class MaterialRequestDto
    {
        public string Noun { get; set; } = string.Empty;
        public string Modifier { get; set; } = string.Empty;
        public string RequestType { get; set; } = "Single";
        public string Plant { get; set; } = "PLT1";
        public Dictionary<string, string> Attributes { get; set; } = new();
        public bool IgnoreSemanticWarning { get; set; }
    }

    /// <summary>DTO for bulk upload — multiple material lines.</summary>
    public class BulkUploadDto
    {
        public List<MaterialRequestDto> Items { get; set; } = new();
    }

    /// <summary>DTO for bulk upload result per line.</summary>
    public class BulkUploadLineResult
    {
        public int LineNumber { get; set; }
        public string Noun { get; set; } = string.Empty;
        public string Modifier { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? RequestRefNo { get; set; }
        public string? Message { get; set; }
        public bool IsDuplicate { get; set; }
    }

    /// <summary>DTO for the approval action.</summary>
    public class ApprovalActionDto
    {
        public int RequestId { get; set; }
        public string Action { get; set; } = "Approve"; // Approve or Reject
        public string Role { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }

    /// <summary>DTO for attribute schema response.</summary>
    public class AttributeSchemaDto
    {
        public string Noun { get; set; } = string.Empty;
        public string Modifier { get; set; } = string.Empty;
        public List<AttributeFieldDto> Fields { get; set; } = new();
    }

    public class AttributeFieldDto
    {
        public string AttributeName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>DTO for Noun-Modifier combination list.</summary>
    public class NounModifierDto
    {
        public string Noun { get; set; } = string.Empty;
        public string Modifier { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    /// <summary>Result of semantic (AI embedding) duplicate check.</summary>
    public class SemanticDuplicateResult
    {
        public bool IsBlocked { get; set; }
        public bool IsSuspicious { get; set; }
        public string Message { get; set; } = string.Empty;
        public double HighestSimilarity { get; set; }
        public List<AiSearchResult> TopMatches { get; set; } = new();
    }

    /// <summary>A single similar item returned from semantic search.</summary>
    public class AiSearchResult
    {
        public string MaterialNumber { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Plant { get; set; } = string.Empty;
        public double Similarity { get; set; }
    }
}
