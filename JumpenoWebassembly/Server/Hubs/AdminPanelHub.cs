using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Constants;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Hubs
{
    public class AdminPanelHub : Hub
    {
        private readonly GameService _gameService;
        //private readonly AnonymUsersService _anonymUsersService;

        public AdminPanelHub(GameService gameService/*, AnonymUsersService anonymUsersService*/)
        {
            _gameService = gameService;
            //_anonymUsersService = anonymUsersService;
        }

        [HubMethodName(AdminPanelHubC.GetStats)]
        public async Task GetActualStats()
        {
            var gamesSettings = _gameService.GetGamesSettings();
            var usersCount = _gameService.GetUsersCount();
            await Clients.Caller.SendAsync(AdminPanelHubC.StatsReceived, gamesSettings, usersCount);
        }
    }
}
