using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Pages
{
    public partial class AdminPanel : IAsyncDisposable
    {
        [Inject] public NavigationManager Navigation { get; set; }

        private HubConnection _hubConnection;
        private List<GameSettings> _gamesSettings = new List<GameSettings>();
        private int _usersCount = 0;

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

            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync(AdminPanelHubC.GetStats);
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
