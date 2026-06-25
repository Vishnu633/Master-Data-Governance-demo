using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hofinsoft.Mdg.Controllers
{
    [ApiController]
    [Route("api/templates")]
    public class MaterialTemplatesController : ControllerBase
    {
        private readonly MdgDbContext _context;

        public MaterialTemplatesController(MdgDbContext context)
        {
            _context = context;
        }

        // GET: api/templates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaterialTemplate>>> GetTemplates()
        {
            return await _context.MaterialTemplates.ToListAsync();
        }

        // POST: api/templates
        [HttpPost]
        public async Task<ActionResult<MaterialTemplate>> CreateTemplate(MaterialTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.Noun) || string.IsNullOrWhiteSpace(template.Modifier))
            {
                return BadRequest(new { Message = "Noun and Modifier are required." });
            }

            // Normalize Noun and Modifier to uppercase for consistency
            template.Noun = template.Noun.Trim().ToUpper();
            template.Modifier = template.Modifier.Trim().ToUpper();

            // Ensure no duplicate templates exist
            var exists = await _context.MaterialTemplates.AnyAsync(t => t.Noun == template.Noun && t.Modifier == template.Modifier);
            if (exists)
            {
                return Conflict($"A template for {template.Noun} {template.Modifier} already exists.");
            }

            // Normalize attribute names
            for (int i = 0; i < template.RequiredAttributes.Count; i++)
            {
                template.RequiredAttributes[i] = template.RequiredAttributes[i].Trim();
            }

            _context.MaterialTemplates.Add(template);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTemplates), new { id = template.Id }, template);
        }
    }
}
