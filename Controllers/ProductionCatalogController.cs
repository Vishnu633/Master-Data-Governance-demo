using Hofinsoft.Mdg.Data;
using Hofinsoft.Mdg.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hofinsoft.Mdg.Controllers
{
    [ApiController]
    [Route("api/production")]
    public class ProductionCatalogController : ControllerBase
    {
        private readonly MdgDbContext _context;

        public ProductionCatalogController(MdgDbContext context)
        {
            _context = context;
        }

        // GET: api/production
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionCatalog>>> GetProductionCatalog()
        {
            return await _context.ProductionCatalog.ToListAsync();
        }

        // GET: api/production/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductionCatalog>> GetProductionItem(int id)
        {
            var item = await _context.ProductionCatalog.FindAsync(id);
            if (item == null)
            {
                return NotFound($"Production item with ID {id} not found.");
            }
            return item;
        }

        // GET: api/production/uid/{uniqueId}
        [HttpGet("uid/{uniqueId}")]
        public async Task<ActionResult<ProductionCatalog>> GetProductionItemByUniqueId(string uniqueId)
        {
            var item = await _context.ProductionCatalog.FirstOrDefaultAsync(p => p.UniqueId == uniqueId);
            if (item == null)
            {
                return NotFound($"Production item with Unique ID '{uniqueId}' not found.");
            }
            return item;
        }
    }
}
