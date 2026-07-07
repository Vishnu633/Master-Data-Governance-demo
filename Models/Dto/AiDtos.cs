using System.Collections.Generic;

namespace Hofinsoft.Mdg.Models.Dto
{
    public class AiClassifyRequest
    {
        public string Description { get; set; } = "";
    }

    public class AiClassificationResult
    {
        public string Noun { get; set; } = "";
        public string Modifier { get; set; } = "";
        public string Plant { get; set; } = "PLT1";
        public Dictionary<string, string> Attributes { get; set; } = new();
        public double Confidence { get; set; }
        public string GeneratedDescription { get; set; } = "";
    }

    public class AiSearchRequest
    {
        public string Query { get; set; } = "";
    }
}
