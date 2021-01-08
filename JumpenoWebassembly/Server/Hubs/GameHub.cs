using JumpenoWebassembly.Server.Components.Jumpeno.Entities;
using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Constants;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Hubs
{
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
            
            var user = await _userService.GetUser();
            var player = new Player { Id = user.Id, Name = user.Username, Skin = "mageSprite_fire" };
            await _gameService.TryAddPlayer(player, code, Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = await _userService.GetUser();
            await _gameService.RemovePlayer(user.Id);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
