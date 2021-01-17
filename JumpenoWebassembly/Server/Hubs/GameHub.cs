using JumpenoWebassembly.Server.Components.Jumpeno.Entities;
using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Game;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Hubs
{
    /// <summary>
    /// Hub for client-server comunication during game
    /// </summary>
    public class GameHub : Hub
    {
        private readonly GameService _gameService;
        private readonly IUserService _userService;

        public GameHub(GameService gameService, IUserService userService)
        {
            _gameService = gameService;
            _userService = userService;
        }

        [HubMethodName(GameHubC.ConnectToLobby)]
        public async Task ConnectToLobby(string code)
        {
            if (!_gameService.ExistGame(code)) return;

            var authMethod = Context.User.FindFirstValue(ClaimTypes.AuthenticationMethod);
            var spectate = authMethod == AuthenticationMethod.Spectator;
            var user = await _userService.GetUser();
            var player = new Player { Id = user.Id, Name = user.Username, Skin = user.Skin ?? Skins.Names[3] };
            var result = await _gameService.ConnectToGame(player, code, Context.ConnectionId, spectate);
            if (!result) {
                await Clients.Caller.SendAsync(GameHubC.LobbyFull);
            }
        }

        [HubMethodName(GameHubC.ChangeLobbyInfo)]
        public async Task ChangeLobbyInfo(LobbyInfo info)
        {
            var user = await _userService.GetUser();
            await _gameService.ChangeLobbyInfo(info, user.Id);
        }

        [HubMethodName(GameHubC.ChangeGameplayInfo)]
        public async Task ChangeGameplayInfo(GameplayInfo info)
        {
            var user = await _userService.GetUser();
            await _gameService.ChangeGameplayInfo(info, user.Id);
        }

        [HubMethodName(GameHubC.StartGame)]
        public async Task StartGame()
        {
            var user = await _userService.GetUser();
            await _gameService.StartGame(user.Id);
        }

        [HubMethodName(GameHubC.DeleteGame)]
        public async Task DeleteGame()
        {
            var user = await _userService.GetUser();
            await _gameService.DeleteGame(user.Id);
        }

        [HubMethodName(GameHubC.ChangePlayerMovement)]
        public async Task ChangePlayerMovement(Enums.MovementDirection direction, bool value)
        {
            var user = await _userService.GetUser();
            await _gameService.ChangePlayerMovement(user.Id, direction, value);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = await _userService.GetUser();
            var gameCode = await _gameService.RemovePlayer(user.Id);
            if (!String.IsNullOrWhiteSpace(gameCode)) {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameCode);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
