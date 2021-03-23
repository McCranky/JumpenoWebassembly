using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Models;
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
        private List<GameSettings> _games = new List<GameSettings>();
        private MeasurePoint _currentMeasure = new MeasurePoint();
        private Timer _timer;
        private bool _gameSection = true;

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                    .WithUrl(Navigation.ToAbsoluteUri(AdminPanelHubC.Url))
                    .Build();

            _hubConnection.On<IEnumerable<GameSettings>>(AdminPanelHubC.ReceiveGames, (games) =>
            {
                _games = games != null ? games.ToList() : _games;
                StateHasChanged();
            });

            _hubConnection.On<GameSettings>(AdminPanelHubC.GameAdded, (gameSettings) =>
            {
                _games.Add(gameSettings);
                StateHasChanged();
            });

            _hubConnection.On<GameSettings>(AdminPanelHubC.GameRemoved, (gameSettings) =>
            {
                var game = _games.FirstOrDefault(g => g.GameCode == gameSettings.GameCode);
                _games.Remove(game);
                StateHasChanged();
            });

            _hubConnection.On<long>(AdminPanelHubC.PlayerJoined, (userId) =>
            {
                ++_currentMeasure.PlayersCount;
                StateHasChanged();
            });

            _hubConnection.On<long>(AdminPanelHubC.PlayerLeft, (userId) =>
            {
                --_currentMeasure.PlayersCount;
                StateHasChanged();
            });
            
            _hubConnection.On<MeasurePoint>(AdminPanelHubC.ReceiveMeasurement, (point) =>
            {
                _currentMeasure = point;
                StateHasChanged();
            });

            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync(AdminPanelHubC.GetGames);
            await _hubConnection.SendAsync(AdminPanelHubC.GetMeasurement);

            _timer = new Timer(3000);
            _timer.Elapsed += async (sender, e) => await _hubConnection.SendAsync(AdminPanelHubC.GetMeasurement);
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

        private double GetMemoryInMB(long memory)
        {
            return Math.Round(memory / 1000000d, 2);
        }
    }
}
