using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Jumpeno;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;

        public GameController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] GameSettings settings)
        {
            var id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            settings.CreatorId = long.Parse(id);

            if (_gameService.TryAddGame(settings, out var code)) {
                return Ok(code);
            }

            return BadRequest();
        }
    }
}
