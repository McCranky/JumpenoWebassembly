using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace JumpenoWebassembly.Client.Shared
{
    public partial class Game : IDisposable
    {
        [Parameter] public Map Map { get; set; }
        [Parameter] public Player Player { get; set; }
        [Parameter] public List<Player> Players { get; set; }
        [Parameter] public List<Platform> Platforms { get; set; }
        [Parameter] public GameplayInfo GameplayInfo { get; set; }
        [Parameter] public GameSettings GameSettings { get; set; }
        [Parameter] public HubConnection Hub { get; set; }

        [Inject] public IJSRuntime JsRuntime { get; set; }

        private Timer _timer;
        private static bool _isSmall;

        protected override void OnInitialized()
        {
            _timer = new Timer(10000.0 / 60);
            _timer.Elapsed += async (sender, e) => await Tick(sender, e);
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private async Task Tick(Object source, ElapsedEventArgs e)
        {
            foreach (var pl in Players) {
                pl.Animation.Update(0);
            }
            await InvokeAsync(StateHasChanged);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender) {
                await JsRuntime.InvokeAsync<object>("SetFocusToGame");
                await JsRuntime.InvokeAsync<object>("WindowResized");

                var visibleArea = await JsRuntime.InvokeAsync<int[]>("GetSize");
                OnBrowserResize(visibleArea[0], visibleArea[1]);
            }
        }

        [JSInvokable]
        public static void OnBrowserResize(int width, int height)
        {
            _isSmall = width < 1050 ? true : false;
            Console.WriteLine($"[{width}:{height}]");
        }

        protected async Task KeyDown(KeyboardEventArgs e)
        {
            if (Player.Spectator) return;
            switch (e.Key) {
                case "ArrowRight":
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Right, true);
                    break;
                case "ArrowLeft":
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Left, true);
                    break;
                case " ":
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Jump, true);
                    break;
            }
        }

        protected async Task KeyUp(KeyboardEventArgs e)
        {
            if (Player.Spectator) return;
            switch (e.Key) {
                case "ArrowRight":
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Right, false);
                    break;
                case "ArrowLeft":
                    await Hub.SendAsync(GameHubC.ChangePlayerMovement, Enums.MovementDirection.Left, false);
                    break;
                case " ":
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

        public void Dispose()
        {
            _timer.Stop();
            _timer.Close();
        }
    }
}
