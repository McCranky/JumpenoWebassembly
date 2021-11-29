using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace JumpenoWebassembly.Server.Services
{
    public class MonitorService
    {
        private readonly GameService _gameService;
        private readonly Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        public MonitorService(GameService gameService, IServiceProvider serviceProvider)
        {
            _gameService = gameService;
            _serviceProvider = serviceProvider;

            _timer = new Timer(1000 * 60 * 1);
            _timer.Elapsed += async (sender, e) => await DoMeasurement(sender, e);
            _timer.Enabled = true;
        }

        public MeasurePoint GetMeasurement()
        {
            using var currentProcess = Process.GetCurrentProcess();
            var appCpu = new PerformanceCounter("Process", "% Processor Time", currentProcess.ProcessName, true);

            return new MeasurePoint
            {
                Date = DateTime.Now,
                CPU = appCpu.NextValue(),
                Memory = currentProcess.PrivateMemorySize64 / (double)currentProcess.WorkingSet64,
                GamesCount = _gameService.GamesCount,
                PlayersCount = _gameService.UsersCount
            };
        }

        private async Task DoMeasurement(object source, ElapsedEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<DataContext>();

            var measurePoint = GetMeasurement();
            await context.Statistics.AddAsync(measurePoint);
            await context.SaveChangesAsync();
        }
    }
}
