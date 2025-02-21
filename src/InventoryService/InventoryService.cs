using System;
using System.Diagnostics;

namespace InventoryService
{
    public sealed class InventoryService : IDisposable
    {
        private readonly Discovery _discovery;

        private readonly EventLog _logger;

        private readonly Stopwatch _stopwatch;

        private readonly WmiScanner _wmiScanner;

        private bool _disposedValue;

        public InventoryService(EventLog logger)
        {
            _logger = logger;
            _wmiScanner = new WmiScanner();
            _discovery = new Discovery();
            _stopwatch = new Stopwatch();
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Refresh()
        {
            Publisher.CheckDependency();

            _logger?.WriteEntry("Reading packages from WMI.", EventLogEntryType.Information);
            var publishedPackages = _wmiScanner.GetAll();

            _logger?.WriteEntry("Running discovery scan.", EventLogEntryType.Information);
            _stopwatch.Reset();
            _stopwatch.Start();
            var discoveredPackages = _discovery.GetAll();
            _stopwatch.Stop();
            _logger?.WriteEntry($"Discovery scan completed. Elapsed time: {_stopwatch.Elapsed}.", EventLogEntryType.Information);

            if (publishedPackages?.Count > 0)
            {
                _logger?.WriteEntry("Cleaning inventory.", EventLogEntryType.Information);
                Publisher.Unpublish(publishedPackages);
            }
            _logger?.WriteEntry("Updating inventory.", EventLogEntryType.Information);
            Publisher.Publish(discoveredPackages);
            _logger?.WriteEntry($"Inventory updated with {discoveredPackages.Count} packages.");
        }

        private void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                _logger.Dispose();
            }

            _disposedValue = true;
        }
    }
}