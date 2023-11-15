using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using WmiProvider;

namespace WindowsService
{
    public sealed class InventoryService
    {
        private readonly EventLog _logger;
        private readonly string _parentKey;
        private List<Package> _previousList;

        private const string PackagesKey = "Packages";

        public InventoryService(EventLog logger, string keyPath)
        {
            _logger = logger;
            _parentKey = keyPath;
        }

        public void Refresh()
        {
            _logger.WriteEntry("Reading fetchedPackages from registry", EventLogEntryType.Information);
            // Read from registry and assign to _previousList

            var packagesInRegistry = ReadFromRegistry();

            if (!string.IsNullOrEmpty(packagesInRegistry))
            {
                _previousList = JsonSerializer.Deserialize<IList<Package>>(packagesInRegistry).ToList<Package>();
            }

            _logger.WriteEntry("Fetching packages", EventLogEntryType.Information);
            var fetchedPackages = GetPackages();

            _logger.WriteEntry("Updating registry", EventLogEntryType.Information);
            var packagesJson = JsonSerializer.Serialize(fetchedPackages);
            WriteToRegistry(packagesJson);

            if (_previousList != null)
            {
                if (_previousList.SequenceEqual(fetchedPackages))
                {
                    // No change
                    return;
                }

                _logger.WriteEntry("Removing previously published fetchedPackages", EventLogEntryType.Information);
                foreach (var item in _previousList)
                {
                    try
                    {
                        InstrumentationManager.Revoke(item);
                    }
                    catch (NullReferenceException)
                    {
                        // Previously no instance was initiated. Continue.
                    }
                }
            }

            _logger.WriteEntry("Publishing fetchedPackages", EventLogEntryType.Information);
            foreach (var item in fetchedPackages)
            {
                try
                {
                    InstrumentationManager.Publish(item);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            _logger.WriteEntry("Caching current fetchedPackages", EventLogEntryType.Information);
            _previousList = fetchedPackages;
        }

        private void WriteToRegistry(string packagesJson)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(_parentKey, true))
            {
                if (key == null)
                {
                    throw new TaskCanceledException("Registry keys not found.");
                }

                key.SetValue(PackagesKey, packagesJson);
            }
        }

        private string ReadFromRegistry()
        {
            string packagesInRegistry;
            using (var key = Registry.LocalMachine.OpenSubKey(_parentKey))
            {
                if (key == null)
                {
                    throw new TaskCanceledException("Registry keys not found.");
                }

                if (key.GetValue(PackagesKey, null) == null)
                {
                    throw new TaskCanceledException("Registry keys not found.");
                }

                packagesInRegistry = key.GetValue(PackagesKey, null).ToString();
            }

            return packagesInRegistry;
        }

        private static List<Package> GetPackages()
        {
            var commandResult = Query.GetAll();

            return commandResult.ToList();
        }
    }
}