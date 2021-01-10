using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Entities;
using JumpenoWebassembly.Shared.Jumpeno.Game;
using JumpenoWebassembly.Shared.Models;
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
        private List<Platform> _platforms;
        private LobbyInfo _lobbyInfo;
        private GameSettings _gameSettings;
        private GameplayInfo _gameplayInfo;
        private Map _map;

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri(GameHubC.Url))
                .Build();

            #region Lobby
            _hubConnection.On<List<Player>, float, GameSettings, LobbyInfo, GameplayInfo>(GameHubC.ConnectedToLobby, (players, myId, settings, info, gameplayInfo) => {
                _me = players.First(pl => pl.Id == myId);
                _players = players;
                _gameSettings = settings;
                _lobbyInfo = info;
                _gameplayInfo = gameplayInfo;
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
            #endregion

            #region Game
            _hubConnection.On(GameHubC.GameDeleted, async () => {
                await LocalStorage.RemoveItemAsync("code");
                Navigation.NavigateTo("/", true);
            });

            _hubConnection.On<GameplayInfo>(GameHubC.GameplayInfoChanged, (info) => {
                _gameplayInfo = info;
                StateHasChanged();
            });

            _hubConnection.On<MapInfo, List<Platform>, List<PlayerPosition>>(GameHubC.PrepareGame, (mapInfo, platforms, playerPositions) => {
                foreach (var player in playerPositions) {
                    var pl = _players.First(pl => pl.Id == player.Id);
                    pl.Position = new Vector2(player.X, player.Y);
                    pl.SetBody();
                    pl.Alive = true;
                }

                //foreach (var pl in _players) {
                //    pl.SetBody();
                //    pl.Alive = true;
                //    pl.Position = new Vector2(0, 0);
                //}
                _platforms = platforms;
                _map = new Map { Info = mapInfo };

                StateHasChanged();
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
