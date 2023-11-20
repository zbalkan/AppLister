using System.Diagnostics;
using InventoryService;

namespace WindowsService
{
    public sealed class InventoryService
    {
        private readonly EventLog _logger;
        private readonly WmiScanner wmiScanner;
        private readonly Discovery discovery;

        public InventoryService(EventLog logger)
        {
            _logger = logger;
            wmiScanner = new WmiScanner();
            discovery = new Discovery();
        }

        public void Refresh()
        {
            _logger.WriteEntry("Reading packages from WMI.", EventLogEntryType.Information);
            var publishedPackages = wmiScanner.GetAll();

            _logger.WriteEntry("Running discovery scan.", EventLogEntryType.Information);
            var discoveredPackages = discovery.GetAll();

            if (publishedPackages?.Count > 0)
            {
                _logger.WriteEntry("Cleaning inventory.", EventLogEntryType.Information);
                Publisher.Unpublish(publishedPackages);
            }
            _logger.WriteEntry("Updating inventory.", EventLogEntryType.Information);
            Publisher.Publish(discoveredPackages);
        }
    }
}