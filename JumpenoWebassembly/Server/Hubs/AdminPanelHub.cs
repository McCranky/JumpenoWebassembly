using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Constants;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Hubs
{
    public class AdminPanelHub : Hub
    {
        private readonly GameService _gameService;
        private readonly MonitorService _monitorService;

        public AdminPanelHub(GameService gameService, MonitorService monitorService)
        {
            _gameService = gameService;
            _monitorService = monitorService;
        }

        [HubMethodName(AdminPanelHubC.GetGames)]
        public async Task GetActualStats()
        {
            var games = _gameService.GetGamesSettings();
            await Clients.Caller.SendAsync(AdminPanelHubC.ReceiveGames, games);
        }

        [HubMethodName(AdminPanelHubC.KickPlayer)]
        public async Task KickPlayer(long playerId, string gameCode)
        {
            await _gameService.KickPlayer(playerId, gameCode);
        }

        [HubMethodName(AdminPanelHubC.DeleteGame)]
        public async Task DeleteGame(string gameCode)
        {
            await _gameService.DeleteGame(gameCode);
        }

        [HubMethodName(AdminPanelHubC.GetMeasurement)]
        public async Task GetUsageStats()
        {
            await Clients.All.SendAsync(AdminPanelHubC.ReceiveMeasurement, _monitorService.GetMeasurement());
        }
    }
}
