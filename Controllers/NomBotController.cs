using Hofinsoft.Mdg.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Hofinsoft.Mdg.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NomBotController : ControllerBase
    {
        private readonly NomBotService _nomBot;

        public NomBotController(NomBotService nomBot)
        {
            _nomBot = nomBot;
        }

        /// <summary>
        /// POST /api/nombot/ask
        /// Ask NomBot a query.
        /// </summary>
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message cannot be empty.");

            var response = await _nomBot.GetAnswerAsync(request.Message);
            return Ok(response);
        }
    }

    public class AskRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
