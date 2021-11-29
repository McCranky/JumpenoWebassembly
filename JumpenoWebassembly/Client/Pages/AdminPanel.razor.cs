using Blazored.Modal;
using Blazored.Modal.Services;
using JumpenoWebassembly.Client.Shared;
using JumpenoWebassembly.Shared;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Models;
using JumpenoWebassembly.Shared.Models.Request;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Timers;

namespace JumpenoWebassembly.Client.Pages
{
    public partial class AdminPanel : IAsyncDisposable
    {
        [Inject] public HttpClient Http { get; set; }
        [Inject] public NavigationManager Navigation { get; set; }
        [CascadingParameter] public IModalService ModalGraph { get; set; }

        private HubConnection _hubConnection;
        private List<GameSettings> _games = new List<GameSettings>();
        private MeasurePoint _currentMeasure = new MeasurePoint();
        private Timer _timer;
        private bool _gameSection = true;
        public DateTime _dateFrom { get; set; } = DateTime.UtcNow;
        public DateTime _dateTo { get; set; } = DateTime.UtcNow;

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                    .WithUrl(Navigation.ToAbsoluteUri(AdminPanelHubC.Url))
                    .Build();

            _hubConnection.On<IEnumerable<GameSettings>>(AdminPanelHubC.ReceiveGames, (games) =>
            {
                _games = games != null ? games.ToList() : _games;
                StateHasChanged();
            });

            _hubConnection.On<GameSettings>(AdminPanelHubC.GameAdded, (gameSettings) =>
            {
                _games.Add(gameSettings);
                StateHasChanged();
            });

            _hubConnection.On<GameSettings>(AdminPanelHubC.GameRemoved, (gameSettings) =>
            {
                var game = _games.FirstOrDefault(g => g.GameCode == gameSettings.GameCode);
                _games.Remove(game);
                StateHasChanged();
            });

            _hubConnection.On<long>(AdminPanelHubC.PlayerJoined, (userId) =>
            {
                ++_currentMeasure.PlayersCount;
                StateHasChanged();
            });

            _hubConnection.On<long>(AdminPanelHubC.PlayerLeft, (userId) =>
            {
                --_currentMeasure.PlayersCount;
                StateHasChanged();
            });

            _hubConnection.On<MeasurePoint>(AdminPanelHubC.ReceiveMeasurement, (point) =>
            {
                _currentMeasure = point;
                StateHasChanged();
            });

            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync(AdminPanelHubC.GetGames);
            await _hubConnection.SendAsync(AdminPanelHubC.GetMeasurement);

            _timer = new Timer(5000);
            _timer.Elapsed += async (sender, e) => await _hubConnection.SendAsync(AdminPanelHubC.GetMeasurement);
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        private void SwitchSection()
        {
            _gameSection = !_gameSection;
        }

        private async Task DeleteGame(string code)
        {
            await _hubConnection.SendAsync(AdminPanelHubC.DeleteGame, code);
        }

        private async Task KickPlayer(long id, string code)
        {
            await _hubConnection.SendAsync(AdminPanelHubC.KickPlayer, id, code);
        }

        private string ToPerentage(double value)
        {
            return value.ToString("0.00");
        }

        private async Task ShowGraph()
        {
            var requestBody = new MeasurementRequest
            {
                From = _dateFrom,
                To = _dateTo
            };
            var result = await Http.PostAsJsonAsync<MeasurementRequest>("api/adminpanel/measurePoints", requestBody);
            var measurements = await result.Content.ReadFromJsonAsync<List<MeasurePoint>>();
            if (measurements == null || measurements.Count == 0) return;
            //var stats = await Http.PostAsJsonAsync<List<MeasurePoint>>("api/adminPanel/measurePoints", requestBody);

            var dates = new List<string>(measurements.Count);
            var players = new List<double>(measurements.Count);
            var games = new List<double>(measurements.Count);
            var cpu = new List<double>(measurements.Count);
            var memory = new List<double>(measurements.Count);

            foreach (var measurement in measurements)
            {
                dates.Add(measurement.Date.ToString());
                players.Add(measurement.PlayersCount);
                games.Add(measurement.GamesCount);
                cpu.Add(measurement.CPU);
                memory.Add(measurement.Memory);
            }

            var data = new List<ChartData>
            {
                new ChartData
                {
                    Label = "Players",
                    BorderColor = "rgb(255, 99, 132)",
                    Data = players
                },
                new ChartData
                {
                    Label = "Games",
                    BorderColor = "rgb(0, 99, 132)",
                    Data = games
                },
                new ChartData
                {
                    Label = "CPU",
                    BorderColor = "rgb(30, 255, 132)",
                    Data = cpu
                },
                new ChartData
                {
                    Label = "Memory",
                    BorderColor = "rgb(255, 99, 132)",
                    Data = memory
                }
            };
            var parameters = new ModalParameters();
            parameters.Add(nameof(Graph.Data), data);
            parameters.Add(nameof(Graph.Labels), dates);

            ModalGraph.Show<Graph>("Stat graph", parameters);
        }
    }
}
