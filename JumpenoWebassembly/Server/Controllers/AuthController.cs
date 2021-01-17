using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Constants;
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
        private readonly AnonymUsersService _anonyms;

        public AuthController(IAuthService authService, AnonymUsersService anonyms)
        {
            _authService = authService;
            _anonyms = anonyms;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest)
        {
            var result = await _authService.Login(loginRequest.Email, loginRequest.Password);
            if (result.Success) {
                // vytvoriť claimi
                var claimEmailAddress = new Claim(ClaimTypes.Email, result.Data.Email);
                var claimName = new Claim(ClaimTypes.Name, result.Data.Username);
                var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, Convert.ToString(result.Data.Id));
                var claimAuthMethod = new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethod.Server);
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
            if (User.FindFirstValue(ClaimTypes.AuthenticationMethod) == AuthenticationMethod.Anonym) {
                _anonyms.RemoveAnonym(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            }

            await HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest registerRequest)
        {
            var user = new User {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                IsConfirmed = registerRequest.IsConfirmed
            };
            var result = await _authService.Register(user, registerRequest.Password);

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
                if (User.FindFirstValue(ClaimTypes.AuthenticationMethod) == AuthenticationMethod.Anonym ||
                    User.FindFirstValue(ClaimTypes.AuthenticationMethod) == AuthenticationMethod.Spectator) {
                    return Ok(new User {
                        Id = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                        Username = User.FindFirstValue(ClaimTypes.Name),
                        IsConfirmed = false
                    });
                }

                user.Email = User.FindFirstValue(ClaimTypes.Email);
                user = await _authService.GetUser(user.Email);

                if (user == null) {
                    user = new User {
                        Id = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                        Username = User.FindFirstValue(ClaimTypes.GivenName),
                        Email = User.FindFirstValue(ClaimTypes.Email),
                        IsConfirmed = true
                    };
                    await _authService.Register(user, user.Email);
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
            var anonym = _anonyms.GetNewAnonym();

            var claimName = new Claim(ClaimTypes.Name, anonym.Name);
            var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, anonym.Id.ToString());
            var claimAuthMethod = new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethod.Anonym);
            // vytvorit claimIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimNameIdentifier, claimName, claimAuthMethod }, "serverAuth");
            // vytvoriť claimsPrincipal
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            // prihlásiť
            await HttpContext.SignInAsync(claimsPrincipal);

            return Redirect("/");
        }

        [HttpGet("spectatorSignIn")]
        public async Task<IActionResult> SpectatorSignIn()
        {
            // claimy
            var claimName = new Claim(ClaimTypes.Name, "_");
            var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, "0");
            var claimAuthMethod = new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethod.Spectator);
            // vytvorit claimIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimNameIdentifier, claimName, claimAuthMethod }, "serverAuth");
            // vytvoriť claimsPrincipal
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            // prihlásiť
            await HttpContext.SignInAsync(claimsPrincipal);

            return Ok();
        }
    }
}
