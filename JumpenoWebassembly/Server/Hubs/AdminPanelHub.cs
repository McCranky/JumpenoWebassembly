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

        [HubMethodName(AdminPanelHubC.GetStats)]
        public async Task GetActualStats()
        {
            var gamesSettings = _gameService.GetGamesSettings();
            var usersCount = _gameService.GetUsersCount();
            await Clients.Caller.SendAsync(AdminPanelHubC.StatsReceived, gamesSettings, usersCount);
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

        [HubMethodName(AdminPanelHubC.GetUsageStats)]
        public async Task GetUsageStats()
        {
            using var currentProcess = Process.GetCurrentProcess();
            var appCpu = new PerformanceCounter("Process", "% Processor Time", currentProcess.ProcessName, true);
            var cpuUsage = appCpu.NextValue();

            System.Console.WriteLine(cpuUsage);
            System.Console.WriteLine(currentProcess.PrivateMemorySize64);

            await Clients.All.SendAsync(AdminPanelHubC.ReceiveUsageStats, cpuUsage, currentProcess.PrivateMemorySize64);
        }
    }
}
