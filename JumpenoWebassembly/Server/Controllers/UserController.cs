using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly DataContext _context;

        public UserController(DataContext context)
        {
            _context = context;
        }

        [HttpPut("updateSkin")]
        public async Task<IActionResult> UpdateSkin([FromBody] User user)
        {
            var dbUser = _context.Users.First(usr => usr.Id == user.Id);
            dbUser.Skin = user.Skin;

            _context.Users.Update(dbUser);
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
