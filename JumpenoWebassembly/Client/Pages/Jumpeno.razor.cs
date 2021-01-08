using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Entities;
using JumpenoWebassembly.Shared.Jumpeno.Game;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace JumpenoWebassembly.Client.Pages
{
    public partial class Jumpeno : IAsyncDisposable
    {
        [Inject] public NavigationManager Navigation { get; set; }
        [Inject] public ILocalStorageService LocalStorage { get; set; }
        [Parameter] public string GameCode { get; set; }

        private HubConnection _hubConnection;

        private Player _me;
        private List<Player> _players;
        private LobbyInfo _lobbyInfo;
        private GameSettings _gameSettings;

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri(GameHubC.Url))
                .Build();

            #region Lobby Methods

            _hubConnection.On<List<Player>, float, GameSettings, LobbyInfo>(GameHubC.ConnectedToLobby, (players, myId, settings, info) => {
                _me = players.First(pl => pl.Id == myId);
                _players = players;
                _gameSettings = settings;
                _lobbyInfo = info;
                StateHasChanged();
            });

            _hubConnection.On<Player>(GameHubC.PlayerJoined, (player) => {
                _players.Add(player);
                StateHasChanged();
            });

            _hubConnection.On<float>(GameHubC.PlayerLeft, (playerId) => {
                var player = _players.First(pl => pl.Id == playerId);
                _players.Remove(player);
                StateHasChanged();
            });

            _hubConnection.On<LobbyInfo>(GameHubC.LobbyInfoChanged, (info) => {
                _lobbyInfo = info;
                StateHasChanged();
            });

            _hubConnection.On<GameSettings>(GameHubC.SettingsChanged, (settings) => {
                _gameSettings = settings;
                StateHasChanged();
            });

            _hubConnection.On(GameHubC.GameDeleted, async () => {
                await LocalStorage.RemoveItemAsync("code");
                Navigation.NavigateTo("/", true);
            });

            #endregion


            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync(GameHubC.ConnectToLobby, GameCode);
        }

        public async ValueTask DisposeAsync()
        {
            await _hubConnection.DisposeAsync();
            await LocalStorage.RemoveItemAsync("code");
        }











    }
}
