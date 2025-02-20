using System;
using System.Collections.Generic;
using System.Linq;
using InventoryEngine.Junk.Confidence;
using InventoryEngine.Junk.Containers;
using InventoryEngine.Tools;

namespace InventoryEngine.Junk.Finders.Registry
{
    internal class AppCompatFlagScanner : IJunkCreator
    {
        private static readonly IEnumerable<string> AppCompatFlags = new[]
        {
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags",
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags"
        };

        public void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
        }

        public IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            if (string.IsNullOrEmpty(target.InstallLocation))
            {
                yield break;
            }

            foreach (var fullCompatKey in AppCompatFlags.SelectMany(compatKey => new[]
            {
                compatKey + @"\Layers",
                compatKey + @"\Compatibility Assistant\Store"
            }))
            {
                using var key = RegistryTools.OpenRegistryKey(fullCompatKey);
                if (key == null)
                {
                    continue;
                }

                foreach (var valueName in key.GetValueNames())
                {
                    // Check for matches
                    if (!valueName.StartsWith(target.InstallLocation,
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    var junk = new RegistryValueJunk(key.Name, valueName, target, this);
                    junk.Confidence.Add(ConfidenceRecords.ExplicitConnection);
                    yield return junk;
                }
            }
        }

        public string CategoryName => "Junk_AppCompat_GroupName";
    }
}