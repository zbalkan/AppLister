using System;
using System.Diagnostics;

namespace InventoryService
{
    public sealed class InventoryService : IDisposable
    {
        private readonly EventLog _logger;
        private readonly Discovery discovery;
        private readonly Stopwatch stopwatch;
        private readonly WmiScanner wmiScanner;
        private bool disposedValue;

        public InventoryService(EventLog logger)
        {
            _logger = logger;
            wmiScanner = new WmiScanner();
            discovery = new Discovery();
            stopwatch = new Stopwatch();
        }

        public void Refresh()
        {
            Publisher.CheckDependency();

            _logger?.WriteEntry("Reading packages from WMI.", EventLogEntryType.Information);
            var publishedPackages = wmiScanner.GetAll();

            _logger?.WriteEntry("Running discovery scan.", EventLogEntryType.Information);
            stopwatch.Reset();
            stopwatch.Start();
            var discoveredPackages = discovery.GetAll();
            stopwatch.Stop();
            _logger?.WriteEntry($"Discovery scan completed. Elapsed time: {stopwatch.Elapsed}.", EventLogEntryType.Information);

            if (publishedPackages?.Count > 0)
            {
                _logger?.WriteEntry("Cleaning inventory.", EventLogEntryType.Information);
                Publisher.Unpublish(publishedPackages);
            }
            _logger?.WriteEntry("Updating inventory.", EventLogEntryType.Information);
            Publisher.Publish(discoveredPackages);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logger.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}