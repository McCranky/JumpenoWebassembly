﻿using JumpenoWebassembly.Shared.Jumpeno;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Services
{
    /// <summary>
    /// Servis pre vytvorenie hry
    /// </summary>
    public class GameService
    {
        private readonly HttpClient _httpClient;

        public GameService(HttpClient http)
        {
            _httpClient = http;
        }

        public async Task<string> CreateGame(GameSettings settings)
        {
            System.Console.WriteLine("Call to api.");
            var result = await _httpClient.PostAsJsonAsync("api/game/create", settings);
            System.Console.WriteLine("After call to api.");
            if (result.StatusCode == System.Net.HttpStatusCode.OK) {
                return await result.Content.ReadAsStringAsync();
            }
            return "";
        }
    }
}
