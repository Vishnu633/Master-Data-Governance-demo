using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Hofinsoft.Mdg.Services
{
    /// <summary>
    /// Generates standardized short/long descriptions from Noun-Modifier-Attributes.
    /// </summary>
    public class DescriptionEngine
    {
        // Maps attribute names to their abbreviated suffixes for short description
        private static readonly Dictionary<string, string> AttributeAbbreviations = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Inside_Diameter", "ID" },
            { "Outside_Diameter", "OD" },
            { "Material", "" },
            { "Material_Grade", "" },
            { "Type", "" },
            { "Thread", "THD" },
            { "Length", "LG" },
            { "Width", "W" },
            { "Height", "H" },
            { "Thickness", "THK" },
        };

        /// <summary>
        /// Generates a short description like: "BEARING, BALL: 20MM ID, 32MM OD, STEEL"
        /// </summary>
        public string GenerateShortDescription(string noun, string modifier, Dictionary<string, string> attributes)
        {
            var header = $"{noun.ToUpper()}, {modifier.ToUpper()}";
            var parts = new List<string>();

            foreach (var kvp in attributes)
            {
                var val = kvp.Value.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(val)) continue;

                if (AttributeAbbreviations.TryGetValue(kvp.Key, out var abbr) && !string.IsNullOrEmpty(abbr))
                {
                    parts.Add($"{val} {abbr}");
                }
                else
                {
                    parts.Add(val);
                }
            }

            return parts.Count > 0
                ? $"{header}: {string.Join(", ", parts)}"
                : header;
        }

        /// <summary>
        /// Generates a detailed long description with attribute labels.
        /// </summary>
        public string GenerateLongDescription(string noun, string modifier, Dictionary<string, string> attributes)
        {
            var header = $"{noun.ToUpper()} {modifier.ToUpper()}";
            var details = attributes
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => $"{FormatAttributeName(kvp.Key)}: {kvp.Value.Trim().ToUpper()}")
                .ToList();

            return details.Count > 0
                ? $"{header} — {string.Join("; ", details)}"
                : header;
        }

        private static string FormatAttributeName(string name)
        {
            return name.Replace("_", " ");
        }
    }
}
