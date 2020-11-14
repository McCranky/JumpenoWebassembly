using JumpenoWebassembly.Shared.Models.Request;
using JumpenoWebassembly.Shared.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserLoginResponse> Login(UserLoginRequest request)
        {
            var result = await _httpClient.PostAsJsonAsync("api/Auth/login", request);
            return await result.Content.ReadFromJsonAsync<UserLoginResponse>();
        }

        public async Task<UserRegisterResponse> Register(UserRegisterRequest request)
        {
            var result = await _httpClient.PostAsJsonAsync("api/Auth/register", request);
            return await result.Content.ReadFromJsonAsync<UserRegisterResponse>();
        }
    }
}
