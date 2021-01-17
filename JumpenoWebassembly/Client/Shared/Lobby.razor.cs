using Blazored.LocalStorage;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Entities;
using JumpenoWebassembly.Shared.Jumpeno.Game;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Shared
{
    public partial class Lobby
    {
        [Parameter] public Player Player { get; set; }
        [Parameter] public LobbyInfo Info { get; set; }
        [Parameter] public GameSettings Settings { get; set; }
        [Parameter] public List<Player> Players { get; set; }
        //[Parameter] public Action StartGame { get; set; }
        //[Parameter] public Action LeaveLobby { get; set; }
        //[Parameter] public Action DeleteGame { get; set; }
        [Parameter] public HubConnection Hub { get; set; }

        [Inject] public NavigationManager Navigation { get; set; }
        [Inject] public ILocalStorageService Storage { get; set; }

        private int GetProgressBar()
        {
            return (int)Math.Ceiling(((double)Players.Count / Settings.PlayersLimit) * 100.0);
        }

        private string GetUrl()
        {
            return Navigation.BaseUri + $"?code={Settings.GameCode}";
        }

        private string GetQRCode()
        {
            return "https://api.qrserver.com/v1/create-qr-code/?data=" + GetUrl() + "&amp;size=150x150";
        }

        private async Task SwitchTimer()
        {
            Info.StoppedStartTimer = !Info.StoppedStartTimer;
            await Hub.SendAsync(GameHubC.ChangeLobbyInfo, Info);
        }

        private async Task SwitchStartTimer()
        {
            Info.StartTimerRunning = !Info.StartTimerRunning;
            await Hub.SendAsync(GameHubC.ChangeLobbyInfo, Info);
        }

        private async Task StartGame()
        {
            await Hub.SendAsync(GameHubC.StartGame);
        }

        private async Task DeleteGame()
        {
            await Hub.SendAsync(GameHubC.DeleteGame);
        }

        private async Task LeaveLobby()
        {
            await Storage.RemoveItemAsync("code");
            Navigation.NavigateTo("/", true);
        }
    }
}
