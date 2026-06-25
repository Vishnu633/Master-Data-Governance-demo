using System.Collections.Generic;

namespace Hofinsoft.Mdg.Models
{
    public class MaterialTemplate
    {
        public int Id { get; set; }
        
        public string Noun { get; set; } = string.Empty;
        
        public string Modifier { get; set; } = string.Empty;
        
        // List of attributes required for this profile (e.g., Inside_Diameter, Material_Grade)
        public List<string> RequiredAttributes { get; set; } = new();
    }
}
