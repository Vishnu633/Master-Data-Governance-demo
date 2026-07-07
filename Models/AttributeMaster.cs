namespace Hofinsoft.Mdg.Models
{
    /// <summary>
    /// Defines the attribute schema for a Noun-Modifier combination.
    /// Each row represents one required attribute for a material profile.
    /// </summary>
    public class AttributeMaster
    {
        public int Id { get; set; }
        public string Noun { get; set; } = string.Empty;
        public string Modifier { get; set; } = string.Empty;
        public string AttributeName { get; set; } = string.Empty;

        /// <summary>Y = mandatory, N = optional</summary>
        public string MandatoryIndicator { get; set; } = "Y";

        /// <summary>Display order within the attribute group</summary>
        public int SortOrder { get; set; }
    }
}
