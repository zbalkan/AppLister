using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using WmiProvider;

namespace WindowsService
{
    public sealed class InventoryService
    {
        private readonly EventLog _logger;
        private List<Package> _previousList;

        public InventoryService(EventLog logger)
        {
            _logger = logger;
        }

        public void Refresh()
        {
            _logger.WriteEntry("Fetching packages", EventLogEntryType.Information);
            var packages = GetPackages();


            if (_previousList != null)
            {
                if (_previousList.SequenceEqual(packages))
                {
                    // No change
                    return;
                }

                _logger.WriteEntry("Removing previously published packages", EventLogEntryType.Information);
                foreach (var item in _previousList)
                {
                    InstrumentationManager.Revoke(item);
                }
            }

            _logger.WriteEntry("Publishing packages", EventLogEntryType.Information);
            foreach (var item in packages)
            {
                InstrumentationManager.Publish(item);
            }

            _logger.WriteEntry("Caching current packages", EventLogEntryType.Information);
            _previousList = packages;
        }

        private static List<Package> GetPackages()
        {
            var commandResult = Query.GetAll();

            return commandResult.ToList();
        }
    }
}