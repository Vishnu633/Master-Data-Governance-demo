using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hofinsoft.Mdg.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttributesController : ControllerBase
    {
        private readonly NomcatDbContext _db;

        public AttributesController(NomcatDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/attributes/profiles
        /// Returns all distinct Noun-Modifier combinations.
        /// </summary>
        [HttpGet("profiles")]
        public async Task<IActionResult> GetProfiles()
        {
            var profiles = await _db.AttributeMaster
                .GroupBy(a => new { a.Noun, a.Modifier })
                .Select(g => new NounModifierDto
                {
                    Noun = g.Key.Noun,
                    Modifier = g.Key.Modifier,
                    Display = $"{g.Key.Noun} — {g.Key.Modifier}"
                })
                .ToListAsync();

            return Ok(profiles);
        }

        /// <summary>
        /// GET /api/attributes/schema?noun=BEARING&modifier=BALL
        /// Returns the attribute schema (fields) for a given Noun-Modifier.
        /// </summary>
        [HttpGet("schema")]
        public async Task<IActionResult> GetSchema([FromQuery] string noun, [FromQuery] string modifier)
        {
            if (string.IsNullOrWhiteSpace(noun) || string.IsNullOrWhiteSpace(modifier))
                return BadRequest("Noun and Modifier are required.");

            var attributes = await _db.AttributeMaster
                .Where(a => a.Noun == noun.Trim().ToUpper() && a.Modifier == modifier.Trim().ToUpper())
                .OrderBy(a => a.SortOrder)
                .Select(a => new AttributeFieldDto
                {
                    AttributeName = a.AttributeName,
                    IsMandatory = a.MandatoryIndicator == "Y",
                    SortOrder = a.SortOrder
                })
                .ToListAsync();

            if (attributes.Count == 0)
                return NotFound($"No attribute schema found for {noun}/{modifier}.");

            return Ok(new AttributeSchemaDto
            {
                Noun = noun.Trim().ToUpper(),
                Modifier = modifier.Trim().ToUpper(),
                Fields = attributes
            });
        }
    }
}
