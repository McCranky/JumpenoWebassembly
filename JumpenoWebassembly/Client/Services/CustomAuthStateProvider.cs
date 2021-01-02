using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService _auth;
        private readonly HttpClient _http;

        public CustomAuthStateProvider(HttpClient http, IAuthService auth)
        {
            _auth = auth;
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = await _auth.GetUser();

            if (user != null) {
                var claims = new List<Claim>();
                // vytvoriť claimi
                claims.Add(new Claim(ClaimTypes.Name, user.Username));
                if (user.IsConfirmed) {
                    claims.Add(new Claim(ClaimTypes.Email, user.Email));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.Id)));
                }
                // vytvorit claimIdentity
                var claimsIdentity = new ClaimsIdentity(claims, "serverAuth");
                // vytvoriť claimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                // prihlásiť
                return new AuthenticationState(claimsPrincipal);
            } else {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
    }
}
