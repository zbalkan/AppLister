using System.Diagnostics;

namespace InventoryService
{
    public sealed class InventoryService
    {
        private readonly EventLog _logger;
        private readonly Discovery discovery;
        private readonly Stopwatch stopwatch;
        private readonly WmiScanner wmiScanner;
        public InventoryService(EventLog logger)
        {
            _logger = logger;
            wmiScanner = new WmiScanner();
            discovery = new Discovery();
            stopwatch = new Stopwatch();
        }

        public void Refresh()
        {
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
    }
}