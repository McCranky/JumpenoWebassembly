using JumpenoWebassembly.Server.Components.Jumpeno.Entities;
using JumpenoWebassembly.Server.Components.Jumpeno.Game;
using JumpenoWebassembly.Server.Hubs;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Game;
using JumpenoWebassembly.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Services
{
    public class GameService
    {
        private readonly ConcurrentDictionary<string, GameEngine> _games;
        public ConcurrentDictionary<long, string> Users { get; set; } = new ConcurrentDictionary<long, string>();
        //private readonly Dictionary<int, string> _users = new Dictionary<int, string>();
        private const int _gameCap = 10;
        public const int _codeLength = 5;
        public const string _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly Random _rndGen;

        private readonly MapTemplateCollection _templateCollection;
        private readonly IHubContext<GameHub> _hub;

        public GameService(IHubContext<GameHub> hubContext)
        {
            _hub = hubContext;
            _rndGen = new Random();
            _games = new ConcurrentDictionary<string, GameEngine>();
            _templateCollection = new MapTemplateCollection();
        }

        public bool ExistGame(string code)
        {
            return _games.TryGetValue(code, out _);
        }

        public bool TryAddGame(GameSettings settings, out string code)
        {
            if (_games.Count >= _gameCap) {
                code = "";
                return false;
            }

            code = GenerateCode();
            settings.GameCode = code;
            settings.GameName = String.IsNullOrEmpty(settings.GameName) ? "Unnamed" : settings.GameName;
            _games.TryAdd(code, new GameEngine(settings,
                _templateCollection,
                _hub));

            return true;
        }

        public async Task StartGame(long userId)
        {
            await _games[Users[userId]].Start();
        }

        public async Task DeleteGame(long userId)
        {
            var code = Users[userId];
            await _hub.Clients.Group(code).SendAsync(GameHubC.GameDeleted);
            _games.TryRemove(code, out _);
        }

        public List<Player> GetPlayers(string code)
        {
            var game = _games[code];
            return game.PlayersInLobby;
        }

        public async Task<bool> TryAddPlayer(Player player, string code, string connectionId)
        {
            var result = await _games[code].AddPlayer(player);
            if (result) {
                Users.TryAdd(player.Id, code);

                await _hub.Groups.AddToGroupAsync(connectionId, code);
                await _hub.Clients.GroupExcept(code, connectionId).SendAsync(GameHubC.PlayerJoined, player);
                await _hub.Clients.Client(connectionId).SendAsync(GameHubC.ConnectedToLobby, _games[code].PlayersInLobby, player.Id, _games[code].Settings, _games[code].LobbyInfo);
            }
            return result;
        }

        public async Task<Player> RemovePlayer(long id)
        {
            var code = Users[id];
            var player = _games[code].GetPlayer(id);
            await _hub.Clients.Group(code).SendAsync(GameHubC.PlayerLeft, player.Id);
            await _games[code].RemovePlayer(player);

            return player;
        }

        public async Task ChangeLobbyInfo(LobbyInfo info, long id)
        {
            var game = _games[Users[id]];
            game.LobbyInfo = info;
            await _hub.Clients.Group(game.Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, info);
        }






        private string GenerateCode()
        {
            string code;
            do {
                code = new string(Enumerable.Repeat(_chars, _codeLength)
                    .Select(s => s[_rndGen.Next(s.Length)]).ToArray());
            } while (_games.ContainsKey(code));
            return code;
        }
    }
}
