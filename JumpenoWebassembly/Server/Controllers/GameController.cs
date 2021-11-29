using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
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
        private readonly DataContext _context;
        private readonly Random _rnd;

        public GameController(GameService gameService, DataContext context)
        {
            _context = context;
            _gameService = gameService;
            _rnd = new Random();
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] GameSettings settings)
        {
            Console.WriteLine("Hello from game/create");
            var id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            settings.CreatorId = long.Parse(id);

            MapTemplate map = null;
            var maps = _context.Maps.ToList();
            if (maps.Count > 0) {
                map = maps[_rnd.Next(0, maps.Count)];
            }

            var code = await _gameService.TryAddGame(settings, map);
            if (!String.IsNullOrWhiteSpace(code)) {
                return Ok(code);
            }

            return BadRequest();
        }

        [HttpGet("maps")]
        public IActionResult GetMaps()
        {
            var maps = _context.Maps.ToList();
            return Ok(maps);
        }

        [HttpGet("delmap/{id}")]
        public async Task<IActionResult> GetMaps([FromRoute] int id)
        {
            var map = _context.Maps.FirstOrDefault(map => map.Id == id);
            if (map != null) {
                _context.Remove(map);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return NotFound();
        }

        [HttpPut("upmap")]
        public async Task<IActionResult> UpdateMap([FromBody] MapTemplate template)
        {
            var map = _context.Maps.FirstOrDefault(map => map.Id == template.Id);
            if (map != null) {
                map.Name = template.Name;
                map.BackgroundColor = template.BackgroundColor;
                map.Width = template.Width;
                map.Height = template.Height;
                map.Tiles = template.Tiles;
                _context.Update(map);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return NotFound();
        }

        [HttpPost("addmap")]
        public async Task<IActionResult> AddMaps([FromBody] MapTemplate template)
        {
            _context.Maps.Add(template);
            await _context.SaveChangesAsync();
            return Ok(template.Id);
        }
    }
}
