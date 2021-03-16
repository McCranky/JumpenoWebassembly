using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace JumpenoWebassembly.Client.Pages
{
    public partial class AdminPanel : IAsyncDisposable
    {
        [Inject] public NavigationManager Navigation { get; set; }

        private HubConnection _hubConnection;
        private List<GameSettings> _gamesSettings = new List<GameSettings>();
        private int _usersCount = 0;
        private float _cpuUsage = 0;
        private double _ramUsage = 0;
        private Timer _timer;

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                    .WithUrl(Navigation.ToAbsoluteUri(AdminPanelHubC.Url))
                    .Build();

            _hubConnection.On<IEnumerable<GameSettings>, int>(AdminPanelHubC.StatsReceived, (gamesSettings, usersCount) =>
            {
                _gamesSettings = gamesSettings != null ? gamesSettings.ToList() : _gamesSettings;
                _usersCount = usersCount;
                StateHasChanged();
            });

            _hubConnection.On<GameSettings>(AdminPanelHubC.GameAdded, (gameSettings) =>
            {
                _gamesSettings.Add(gameSettings);
                StateHasChanged();
            });

            _hubConnection.On<GameSettings>(AdminPanelHubC.GameRemoved, (gameSettings) =>
            {
                var game = _gamesSettings.FirstOrDefault(g => g.GameCode == gameSettings.GameCode);
                _gamesSettings.Remove(game);
                StateHasChanged();
            });

            _hubConnection.On<long>(AdminPanelHubC.PlayerJoined, (userId) =>
            {
                _usersCount += 1;
                StateHasChanged();
            });

            _hubConnection.On<long>(AdminPanelHubC.PlayerLeft, (userId) =>
            {
                _usersCount -= 1;
                StateHasChanged();
            });
            
            _hubConnection.On<float, long>(AdminPanelHubC.ReceiveUsageStats, (cpu, ram) =>
            {
                _cpuUsage = cpu;
                _ramUsage = Math.Round(ram / 1000000d, 2);
                Console.WriteLine($"CPU: {cpu}; RAM: {ram}");
                StateHasChanged();
            });

            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync(AdminPanelHubC.GetStats);
            await _hubConnection.SendAsync(AdminPanelHubC.GetUsageStats);

            _timer = new Timer(1000);
            _timer.Elapsed += async (sender, e) => await _hubConnection.SendAsync(AdminPanelHubC.GetUsageStats);
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        private async Task DeleteGame(string code)
        {
            await _hubConnection.SendAsync(AdminPanelHubC.DeleteGame, code);
        }

        private async Task KickPlayer(long id, string code)
        {
            await _hubConnection.SendAsync(AdminPanelHubC.KickPlayer, id, code);
        }
    }
}
