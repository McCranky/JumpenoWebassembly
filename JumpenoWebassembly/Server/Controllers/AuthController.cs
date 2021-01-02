using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Models;
using JumpenoWebassembly.Shared.Models.Request;
using JumpenoWebassembly.Shared.Models.Response;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest, bool anonym = false)
        {
            var result = await _authService.Login(loginRequest.Email, loginRequest.Password);
            if (result.Success) {
                // vytvoriť claimi
                var claimEmailAddress = new Claim(ClaimTypes.Email, result.Data.Email);
                var claimName = new Claim(ClaimTypes.Name, result.Data.Username);
                var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, Convert.ToString(result.Data.Id));
                var claimAuthMethod = new Claim(ClaimTypes.AuthenticationMethod, "Server");
                // vytvorit claimIdentity
                var claimsIdentity = new ClaimsIdentity(new[] { claimEmailAddress, claimName, claimNameIdentifier, claimAuthMethod }, "serverAuth");
                // vytvoriť claimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                // prihlásiť
                await HttpContext.SignInAsync(claimsPrincipal);

                return Ok(new UserLoginResponse { User = result.Data });
            }
            return BadRequest(new UserLoginResponse { Message = result.Message });
        }

        [HttpGet("logout")]
        public async Task<ActionResult> LogOutUser()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest registerRequest)
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

        [HttpGet("getCurrentUser")]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            var user = new User();

            if (User.Identity.IsAuthenticated) {
                if (User.FindFirstValue(ClaimTypes.AuthenticationMethod) == "Anonym") {
                    return Ok(new User { Username = User.FindFirstValue(ClaimTypes.Name), IsConfirmed = false });
                }

                user.Email = User.FindFirstValue(ClaimTypes.Email);
                user = await _authService.GetUser(user.Email);

                if (user == null) {
                    user = new User {
                        Username = User.FindFirstValue(ClaimTypes.GivenName),
                        Email = User.FindFirstValue(ClaimTypes.Email),
                        IsConfirmed = true,
                        DateOfBirth = DateTime.Now
                    };
                    await _authService.Register(user, user.Email, 0); await _authService.Register(user, user.Email, 0);
                }
                return Ok(user);
            }

            return BadRequest();
        }

        [HttpGet("facebookSignIn")]
        public async Task FacebookSignIn()
        {
            await HttpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = "/" });
        }

        [HttpGet("googleSignIn")]
        public async Task GoogleSignIn()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = "/" });
        }
        
        [HttpGet("anonymSignIn")]
        public async Task<IActionResult> AnonymSignIn()
        {
            var claimName = new Claim(ClaimTypes.Name, "Levak Bob");
            var claimAuthMethod = new Claim(ClaimTypes.AuthenticationMethod, "Anonym");
            // vytvorit claimIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimName, claimAuthMethod }, "serverAuth");
            // vytvoriť claimsPrincipal
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            // prihlásiť
            await HttpContext.SignInAsync(claimsPrincipal);
            return Redirect("/");
        }
    }
}
