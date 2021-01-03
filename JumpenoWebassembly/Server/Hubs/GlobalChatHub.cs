using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        [HubMethodName(GlobalChatHubC.SEND_MESSAGE)]
        public async Task SendMessage(string message)
        {
            var user = await _userService.GetUser();

            await Clients.All.SendAsync(GlobalChatHubC.RECEIVE_MESSAGE ,user.Username, message);
        }
    }
}
