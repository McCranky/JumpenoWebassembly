using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Shared.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace JumpenoWebassembly.Server.Services
{
    public class MonitorService
    {
        private readonly GameService _gameService;
        private readonly DataContext _context;
        private readonly Timer _timer;

        public MonitorService(GameService gameService, DataContext context)
        {
            _gameService = gameService;
            _context = context;

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
                Memory = currentProcess.PrivateMemorySize64,
                GamesCount = _gameService.GamesCount,
                PlayersCount = _gameService.UsersCount
            };
        }

        private async Task DoMeasurement(object source, ElapsedEventArgs e)
        {
            var measurePoint = GetMeasurement();
            await _context.Statistics.AddAsync(measurePoint);
            await _context.SaveChangesAsync();
        }
    }
}
