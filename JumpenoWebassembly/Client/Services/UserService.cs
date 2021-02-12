using JumpenoWebassembly.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Services
{
    /// <summary>
    /// Servis pre aktualizovanie udajov hraca
    /// </summary>
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient http)
        {
            _httpClient = http;
        }

        public async Task UpdateSkin(User user)
        {
            await _httpClient.PutAsJsonAsync("api/user/updateSkin", user);
        }
    }
}
