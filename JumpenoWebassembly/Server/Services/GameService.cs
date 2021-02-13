using JumpenoWebassembly.Server.Components.Jumpeno.Entities;
using JumpenoWebassembly.Server.Components.Jumpeno.Game;
using JumpenoWebassembly.Server.Hubs;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Game;
using JumpenoWebassembly.Shared.Jumpeno.Utilities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Services
{
    /// <summary>
    /// Servis pre hernu komunikaciu
    /// </summary>
    public class GameService
    {
        public const string _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public const int _codeLength = 5;
        private const int _gameCap = 10;
        private readonly Random _rndGen;

        private readonly ConcurrentDictionary<string, GameEngine> _games;
        private readonly ConcurrentDictionary<long, string> _users;

        private readonly IHubContext<GameHub> _hub;

        public GameService(IHubContext<GameHub> hubContext)
        {
            _hub = hubContext;
            _rndGen = new Random();
            _games = new ConcurrentDictionary<string, GameEngine>();
            _users = new ConcurrentDictionary<long, string>();
        }

        /// <summary>
        /// Skontroluje ci existuje hra s danym kodom
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool ExistGame(string code)
        {
            return _games.TryGetValue(code, out _);
        }

        /// <summary>
        /// Ak je miesto tak prida hru.
        /// Maximalna kapacita prebiehajucich hier je 10.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="map"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool TryAddGame(GameSettings settings, MapTemplate map, out string code)
        {
            if (_games.Count >= _gameCap)
            {
                code = "";
                return false;
            }

            code = GenerateCode();
            settings.GameCode = code;
            settings.GameName = String.IsNullOrEmpty(settings.GameName) ? "Unnamed" : settings.GameName;
            _games.TryAdd(code, new GameEngine(settings,
                map,
                _hub));

            return true;
        }

        public async Task StartGame(long userId)
        {
            await _games[_users[userId]].Start();
        }

        public async Task DeleteGame(long userId)
        {
            var code = _users[userId];
            await _hub.Clients.Group(code).SendAsync(GameHubC.GameDeleted);
            _games.TryRemove(code, out _);
        }

        /// <summary>
        /// Pripojenie hraca do hry s danym kodom.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="code"></param>
        /// <param name="connectionId"></param>
        /// <param name="spectate"></param>
        /// <returns></returns>
        public async Task<bool> ConnectToPlay(Player player, string code, string connectionId)
        {
            if (_games[code].Settings.GameMode == Enums.GameMode.Guided)
            {
                // ak je mod Guided, tak každy musi mať anonymne meno
                player.Name = Usernames.UserNames[_rndGen.Next(Usernames.UserNames.Length)];

                // ak ide o zakladatela, tak ho pripoj ako spectatora
                if (_games[code].Settings.CreatorId == player.Id)
                {
                    await ConnectToSpectate(player.Id, code, connectionId);
                    return true;
                }
            }

            var result = await _games[code].AddPlayer(player);
            if (result)
            {
                await SubscribeToGame(player.Id, code, connectionId);
                await _hub.Clients.GroupExcept(code, connectionId).SendAsync(GameHubC.PlayerJoined, player);
            }

            return result;
        }

        public async Task ConnectToSpectate(long id, string code, string connectionId)
        {
            await SubscribeToGame(id, code, connectionId);
        }

        private async Task SubscribeToGame(long playerId, string gameCode, string connectionId)
        {
            _users.TryAdd(playerId, gameCode);
            await _hub.Groups.AddToGroupAsync(connectionId, gameCode);
            await _hub.Clients.Client(connectionId).SendAsync
                    (
                        GameHubC.ConnectedToLobby,
                        _games[gameCode].PlayersInLobby,
                        playerId,
                        _games[gameCode].Settings,
                        _games[gameCode].LobbyInfo,
                        _games[gameCode].Gameplay
                    );
        }

        /// <summary>
        /// Odpoji hraca z hry.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> RemovePlayer(long id)
        {
            _users.TryGetValue(id, out var code);
            if (code == default) return null;

            _users.TryRemove(id, out _);
            _games.TryGetValue(code, out var game);
            if (game == default) return null;

            var player = game.GetPlayer(id);
            if (player != null)
            {
                await _hub.Clients.Group(code).SendAsync(GameHubC.PlayerLeft, player.Id);
                await game.RemovePlayer(player);
            }

            return code;
        }

        public async Task ChangeLobbyInfo(LobbyInfo info, long id)
        {
            var game = _games[_users[id]];
            game.LobbyInfo = info;
            await _hub.Clients.Group(game.Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, info);
        }

        public async Task ChangeGameplayInfo(GameplayInfo info, long id)
        {
            var game = _games[_users[id]];
            game.Gameplay = info;
            await _hub.Clients.Group(game.Settings.GameCode).SendAsync(GameHubC.GameplayInfoChanged, info);
        }

        public async Task ChangePlayerMovement(long id, Enums.MovementDirection direction, bool value)
        {
            var game = _games[_users[id]];
            game.GetPlayer(id).SetMovement(direction, value);
            await _hub.Clients.Group(game.Settings.GameCode).SendAsync(GameHubC.PlayerMovementChanged, id, direction);
        }


        private string GenerateCode()
        {
            string code;
            do
            {
                code = new string(Enumerable.Repeat(_chars, _codeLength)
                    .Select(s => s[_rndGen.Next(s.Length)]).ToArray());
            } while (_games.ContainsKey(code));
            return code;
        }
    }
}
