using JumpenoWebassembly.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Hubs
{
    [Authorize]
    public class GlobalChatHub : Hub
    {
        private readonly IUserService _userService;

        public GlobalChatHub(IUserService userService)
        {
            _userService = userService;
        }
        public async Task SendMessage(string message)
        {
            var user = await _userService.GetUser();
            await Clients.All.SendAsync("ReceiveMessage", user.Username, message);
        }
    }
}
