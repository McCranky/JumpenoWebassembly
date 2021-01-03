using JumpenoWebassembly.Shared.Jumpeno.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService _auth;
        private readonly Player _player;

        public CustomAuthStateProvider(IAuthService auth, Player player)
        {
            _auth = auth;
            _player = player;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = await _auth.GetUser();

            if (user != null) {
                // vytvoriť claimi
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.Username)
                };

                if (user.IsConfirmed) {
                    claims.Add(new Claim(ClaimTypes.Email, user.Email));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.Id)));
                }

                // vytvorit claimIdentity
                var claimsIdentity = new ClaimsIdentity(claims, "serverAuth");

                // vytvoriť claimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                if (user.Username == "_") {
                    _player.Spectator = true;
                }
                // prihlásiť
                return new AuthenticationState(claimsPrincipal);

            } else {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
    }
}
