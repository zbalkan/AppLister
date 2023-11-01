using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace WingetService
{
    public class WindowsBackgroundService : BackgroundService
    {
        private readonly ILogger<WindowsBackgroundService> _logger;
        private readonly WingetService _wingetService;
        private readonly int _queryPeriodInMilliseconds;

        private const int DefaultQueryPeriodInMinutes = 600;  // Default period constant
        private const string QueryPeriodKey = "QueryPeriodInMinutes";
        private const string ServiceKeyPath = @"SOFTWARE\zb\WingetService";

        public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger, WingetService wingetService)
        {
            _logger = logger;
            _wingetService = wingetService;

            using var key = Registry.LocalMachine.OpenSubKey(ServiceKeyPath) ?? throw new TaskCanceledException("Registry keys not found.");
            if (key.GetValue(QueryPeriodKey, DefaultQueryPeriodInMinutes) is int period)
            {
                _queryPeriodInMilliseconds = period * 60 * 1000;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _wingetService.FetchAndSaveWingetPackages();
                _logger.LogInformation("WindowsBackgroundService running at: {time}", DateTimeOffset.Now);
                await Task.Delay(_queryPeriodInMilliseconds, stoppingToken);
            }
        }
    }
}