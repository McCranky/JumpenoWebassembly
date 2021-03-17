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

        public AdminPanelHub(GameService gameService)
        {
            _gameService = gameService;
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
            using var currentProcess = Process.GetCurrentProcess();
            var appCpu = new PerformanceCounter("Process", "% Processor Time", currentProcess.ProcessName, true);
            var cpuUsage = appCpu.NextValue();

            System.Console.WriteLine(cpuUsage);
            System.Console.WriteLine(currentProcess.PrivateMemorySize64);

            await Clients.All.SendAsync(AdminPanelHubC.ReceiveMeasurement, cpuUsage, currentProcess.PrivateMemorySize64);
        }
    }
}
