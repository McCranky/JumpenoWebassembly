using Blazored.LocalStorage;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Utilities;
using JumpenoWebassembly.Shared.Jumpeno.Entities;
using JumpenoWebassembly.Shared.Jumpeno.Game;
using JumpenoWebassembly.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private bool _isFull;

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri(GameHubC.Url))
                .Build();

            #region Lobby
            _hubConnection.On(GameHubC.LobbyFull, () => {
                _isFull = true;
                StateHasChanged();
            });

            _hubConnection.On<List<Player>, long, GameSettings, LobbyInfo, GameplayInfo>(GameHubC.ConnectedToLobby, (players, myId, settings, info, gameplayInfo) => {
                _me = players.FirstOrDefault(pl => pl.Id == myId);
                if (_me == null) {
                    _me = new Player { Id = myId, Spectator = true };
                }
                _players = players;
                _gameSettings = settings;
                _lobbyInfo = info;
                _gameplayInfo = gameplayInfo;
                StateHasChanged();
            });
            
            _hubConnection.On<long>(GameHubC.PlayerKicked, (id) => {
                if (id == _me.Id)
                {
                    Navigation.NavigateTo("/");
                }
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

            _hubConnection.On<float>(GameHubC.MapShrinked, (shrinkSize) => {
                _map.Info.X -= shrinkSize;
                foreach (var platform in _platforms)
                {
                    platform.X -= shrinkSize / 2f;
                }
                foreach (var player in _players)
                {
                    player.Position = new Vector(player.Position.X - shrinkSize / 2f, player.Position.Y);
                }


                StateHasChanged();
            });

            _hubConnection.On<GameplayInfo>(GameHubC.GameplayInfoChanged, (info) => {
                _gameplayInfo = info;
                StateHasChanged();
            });

            _hubConnection.On<PlayerPosition>(GameHubC.PlayerMoved, (player) => {
                var pl = _players.First(pl => pl.Id == player.Id);
                pl.Position = new Vector(player.X, player.Y);
                pl.FacingRight = player.FacingRight;

                if (pl.Animation.State != player.State) {
                    pl.Animation.State = player.State;
                }

                StateHasChanged();
            });

            _hubConnection.On<float, float>(GameHubC.PlayerDied, (killedId, killerId) => {
                var killed = _players.First(pl => pl.Id == killedId);
                var killer = _players.First(pl => pl.Id == killerId);
                killed.Alive = false;
                killed.Die();
                killed.Animation.Update(0);
                ++killer.Kills;
                //StateHasChanged();
            });

            _hubConnection.On<float>(GameHubC.PlayerCrushed, (id) => {
                var pl = _players.First(pl => pl.Id == id);
                pl.Die();
                pl.Animation.Update(0);
                //StateHasChanged();
            });

            _hubConnection.On<MapInfo, List<Platform>, List<PlayerPosition>>(GameHubC.PrepareGame, (mapInfo, platforms, playerPositions) => {
                foreach (var player in playerPositions) {
                    var pl = _players.First(pl => pl.Id == player.Id);
                    pl.Position = new Vector(player.X, player.Y);
                    pl.SetBody();
                    pl.Alive = true;
                }
                _platforms = platforms;
                _map = new Map { Info = mapInfo };

                StateHasChanged();
            });

            #endregion


            _hubConnection.On<string>("TestCall", (testMessage) => {
                Console.WriteLine(testMessage);
            });

            _hubConnection.On<Player, long, GameSettings, LobbyInfo, GameplayInfo>("Test1", (players, myId, settings, info, gameplayInfo) => {
                //_me = players.FirstOrDefault(pl => pl.Id == myId);
                if (_me == null)
                {
                    _me = new Player { Id = myId, Spectator = true };
                }
                //_players = players;
                Console.WriteLine(players.Alive);
                _gameSettings = settings;
                _lobbyInfo = info;
                _gameplayInfo = gameplayInfo;
                StateHasChanged();
            });




            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync(GameHubC.ConnectToLobby, GameCode);
            await LocalStorage.RemoveItemAsync("code");
        }

        public async ValueTask DisposeAsync()
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
