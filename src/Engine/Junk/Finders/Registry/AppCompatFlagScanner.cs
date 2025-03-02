using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Junk.Confidence;
using Engine.Junk.Containers;
using Engine.Tools;

namespace Engine.Junk.Finders.Registry
{
    internal class AppCompatFlagScanner : IJunkCreator
    {
        public string CategoryName => "Junk_AppCompat_GroupName";

        private static readonly IEnumerable<string> AppCompatFlags = new[]
                {
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags",
            @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags"
        };

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

        public void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
        }
    }
}