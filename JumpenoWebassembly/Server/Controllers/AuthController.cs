using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Models.Request;
using JumpenoWebassembly.Shared.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JumpenoWebassembly.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest)
        {
            var result = await _authService.Login(loginRequest.Email, loginRequest.Password);
            if (result.Success) {
                return Ok(new UserLoginResponse { JwtToken = result.Data });
            }
            return BadRequest(new UserLoginResponse { Message = result.Message });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserRegisterRequest registerRequest)
        {
            var user = new User {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                IsConfirmed = registerRequest.IsConfirmed,
                DateOfBirth = registerRequest.DateOfBirth
            };
            var result = await _authService.Register(user, registerRequest.Password, int.Parse(registerRequest.StartSkinId));

            if (result.Success) {
                return Ok(new UserRegisterResponse { Id = result.Data });
            }
            return BadRequest(new UserRegisterResponse { Message = result.Message });
        }
    }
}
