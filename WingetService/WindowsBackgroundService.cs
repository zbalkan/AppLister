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

        private readonly int _queryPeriodInMinutes;
        private const int DefaultQueryPeriodInMinutes = 600;  // Default period constant
        private const string QueryPeriodKey = "QueryPeriodInMinutes";
        private const string ServiceKeyPath = @"SOFTWARE\zb\WingetService";

        public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger, WingetService wingetService)
        {
            _logger = logger;
            _wingetService = wingetService;

            // Read query period and memory-mapped file path from registry
            using (var key = Registry.LocalMachine.OpenSubKey(ServiceKeyPath))
            {
                if (key?.GetValue(QueryPeriodKey) is string period)
                {
                    _queryPeriodInMinutes = Convert.ToInt32(period);
                }
            }

            // Set default period if not found in registry
            if (_queryPeriodInMinutes <= 0)
            {
                _queryPeriodInMinutes = DefaultQueryPeriodInMinutes;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _wingetService.FetchAndSaveWingetPackages();
                _logger.LogInformation("WindowsBackgroundService running at: {time}", DateTimeOffset.Now);
                await Task.Delay(_queryPeriodInMinutes, stoppingToken);
            }
        }
    }
}
