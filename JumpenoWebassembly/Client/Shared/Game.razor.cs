using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JumpenoWebassembly.Shared.Jumpeno.Utilities;
using JumpenoWebassembly.Shared.Jumpeno.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using JumpenoWebassembly.Shared.Jumpeno;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.SignalR.Client;
using JumpenoWebassembly.Shared.Constants;

namespace JumpenoWebassembly.Client.Shared
{
    public partial class Game
    {
        [Parameter] public Map Map { get; set; }
        [Parameter] public Player Player { get; set; }
        [Parameter] public List<Player> Players { get; set; }
        [Parameter] public List<Platform> Platforms { get; set; }
        [Parameter] public GameplayInfo GameplayInfo { get; set; }
        [Parameter] public GameSettings GameSettings { get; set; }
        [Parameter] public HubConnection Hub { get; set; }

        [Inject] public IJSRuntime JsRuntime { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender) {
                await JsRuntime.InvokeAsync<object>("SetFocusToGame");
            }
        }

        [JSInvokable]
        public async Task OnBrowserResize()
        {
            var visibleArea = await JsRuntime.InvokeAsync<int[]>("GetSize");
            //Width = visibleArea[0];
            //Height = visibleArea[1];
            if (visibleArea[0] < 1050) {
                if (Player != null) {
                    Player.SmallScreen = true;
                }
            } else {
                if (Player != null) {
                    Player.SmallScreen = false;
                }
            }
            StateHasChanged();
        }

        protected async Task KeyDown(KeyboardEventArgs e)
        {
            if (Player.Spectator) return;
            switch (e.Key) {
                case "ArrowRight":
                    //Player.SetMovement(MovementAction.RIGHT, true);
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Right, true);
                    break;
                case "ArrowLeft":
                    //Player.SetMovement(MovementAction.LEFT, true);
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Left, true);
                    break;
                case " ":
                    //Player.SetMovement(MovementAction.JUMP, true);
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Jump, true);
                    break;
            }
        }

        protected async Task KeyUp(KeyboardEventArgs e)
        {
            if (Player.Spectator) return;
            switch (e.Key) {
                case "ArrowRight":
                    //Player.SetMovement(MovementAction.RIGHT, false);
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Right, false);
                    break;
                case "ArrowLeft":
                    //Player.SetMovement(MovementAction.LEFT, false);
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Left, false);
                    break;
                case " ":
                    //Player.SetMovement(MovementAction.JUMP, false);
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Jump, false);
                    break;
            }
        }

        private async Task Left(bool release)
        {
            if (release) {
                await KeyUp(new KeyboardEventArgs() { Key = "ArrowLeft" });
            } else {
                await KeyDown(new KeyboardEventArgs() { Key = "ArrowLeft" });
            }
        }
        private async Task Right(bool release)
        {
            if (release) {
                await KeyUp(new KeyboardEventArgs() { Key = "ArrowRight" });
            } else {
                await KeyDown(new KeyboardEventArgs() { Key = "ArrowRight" });
            }
        }
        private async Task Up(bool release)
        {
            if (release) {
                await KeyUp(new KeyboardEventArgs() { Key = " " });
            } else {
                await KeyDown(new KeyboardEventArgs() { Key = " " });
            }
        }

        private async Task SwitchCountdownTimer()
        {
            GameplayInfo.CountdownTimerRunning = !GameplayInfo.CountdownTimerRunning;
            await Hub.SendAsync(GameHubC.ChangeGameplayInfo, GameplayInfo);
        }

        private async Task SwitchGameoverTimer()
        {
            GameplayInfo.GameoverTimerRunning = !GameplayInfo.GameoverTimerRunning;
            await Hub.SendAsync(GameHubC.ChangeGameplayInfo, GameplayInfo);
        }

        private async Task SwitchShrinking()
        {
            GameplayInfo.ShrinkingAllowed = !GameplayInfo.ShrinkingAllowed;
            await Hub.SendAsync(GameHubC.ChangeGameplayInfo, GameplayInfo);
        }

        private async Task SkipMainPhase()
        {
            GameplayInfo.State = Enums.GameState.Shrinking;
            await Hub.SendAsync(GameHubC.ChangeGameplayInfo, GameplayInfo);
        }

        private async Task SkipGameoverPhase()
        {
            GameplayInfo.FramesToScoreboard = 0;
            await Hub.SendAsync(GameHubC.ChangeGameplayInfo, GameplayInfo);
        }
    }
}
