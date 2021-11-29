using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Shared.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminPanelController : ControllerBase
    {
        private readonly DataContext _context;
        public AdminPanelController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("measurePoints")]
        public async Task<IActionResult> MeasurePoints([FromBody] MeasurementRequest request)
        {
            var stats = await _context.Statistics
                .Where(st => st.Date <= request.To && st.Date >= request.From)
                .ToListAsync();

            return Ok(stats);
        }
    }
}
