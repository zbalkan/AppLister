using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using InventoryEngine.Extensions;
using InventoryEngine.Junk.Confidence;
using InventoryEngine.Junk.Containers;
using InventoryEngine.Tools;
using UninstallTools.Junk.Finders;

namespace InventoryEngine.Junk.Finders.Registry
{
    internal class HeapLeakDetectionScanner : IJunkCreator
    {
        private const string RegKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\RADAR\HeapLeakDetection\DiagnosedApplications";

        private Dictionary<string, string> _lookup;

        public void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
            try
            {
                using (var key = RegistryTools.OpenRegistryKey(RegKey))
                {
                    if (key != null)
                    {
                        // Reg key names are case insensitive so using tolower key is fine
                        _lookup = key.GetSubKeyNames().ToDictionary(x => x.ToLower(), x => x);
                    }
                }
            }
            catch (SystemException ex)
            {
                Debug.WriteLine($"Failed to setup {CategoryName} junk scanner: {ex}");
            }
        }

        public IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            if (_lookup == null || target.SortedExecutables == null || target.SortedExecutables.Length == 0)
            {
                return Enumerable.Empty<IJunkResult>();
            }

            return target.SortedExecutables.Attempt(x =>
                {
                    _lookup.TryGetValue(Path.GetFileName(x).ToLower(), out var hit);
                    return hit;
                })
                .Where(x => x != null)
                .Select(x =>
                {
                    var junk = new RegistryKeyJunk(Path.Combine(RegKey, x), target, this);
                    junk.Confidence.Add(ConfidenceRecords.ExplicitConnection);
                    return junk;
                });
        }

        public string CategoryName => "HeapLeakDetection";
    }
}