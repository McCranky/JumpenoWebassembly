using JumpenoWebassembly.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(DataContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContextAccessor = httpContext;
        }

        public async Task<User> GetUser()
        {
            var res = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = int.Parse(res);
            return await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
        }
    }
}
