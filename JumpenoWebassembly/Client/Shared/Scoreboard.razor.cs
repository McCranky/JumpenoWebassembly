using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Shared
{
    public partial class Scoreboard
    {
        [Parameter] public Player Player { get; set; }
        [Parameter] public List<Player> Players { get; set; }
        [Parameter] public GameSettings GameSettings { get; set; }
        [Parameter] public GameplayInfo GameplayInfo { get; set; }
        [Parameter] public HubConnection Hub { get; set; }

        private async Task SkipPhase()
        {
            GameplayInfo.FramesToLobby = 0;
            await Hub.SendAsync(GameHubC.ChangeGameplayInfo, GameplayInfo);
        }

        private async Task SwitchScoreboardTimer()
        {
            GameplayInfo.ScoreboardTimerRunning = !GameplayInfo.ScoreboardTimerRunning;
            await Hub.SendAsync(GameHubC.ChangeGameplayInfo, GameplayInfo);
        }
    }
}
