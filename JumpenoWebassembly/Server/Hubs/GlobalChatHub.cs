using JumpenoWebassembly.Server.Services;
using JumpenoWebassembly.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Hubs
{
    /// <summary>
    /// Hub pre komunikaciu v globalnom chate.
    /// </summary>
    [Authorize]
    public class GlobalChatHub : Hub
    {
        private readonly IUserService _userService;

        public GlobalChatHub(IUserService userService)
        {
            _userService = userService;
        }

        [HubMethodName(GlobalChatHubC.SendMessage)]
        public async Task SendMessage(string message)
        {
            var user = await _userService.GetUser();

            await Clients.All.SendAsync(GlobalChatHubC.ReceiveMessage, user.Username, message);
        }
    }
}
