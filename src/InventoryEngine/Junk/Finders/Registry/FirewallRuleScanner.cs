using System;
using System.Collections.Generic;
using System.Diagnostics;
using InventoryEngine.Extensions;
using InventoryEngine.Junk.Confidence;
using InventoryEngine.Junk.Containers;
using InventoryEngine.Tools;

namespace InventoryEngine.Junk.Finders.Registry
{
    internal class FirewallRuleScanner : JunkCreatorBase
    {
        private const string FirewallRulesKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\FirewallRules";

        public override IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            var results = new List<IJunkResult>();
            if (string.IsNullOrEmpty(target.InstallLocation))
            {
                return results;
            }

            using var key = GetFirewallRulesKey();

            if (key == null)
            {
                return results;
            }

            foreach (var valueName in key.TryGetValueNames())
            {
                var value = key.GetStringSafe(valueName);
                if (string.IsNullOrEmpty(value)) continue;

                var appIndex = value.IndexOf("|App=", StringComparison.InvariantCultureIgnoreCase);
                var start = appIndex + 5;
                if (appIndex == -1 || start >= value.Length) continue;

                var charCount = value.IndexOf('|', start) - start;
                if (charCount <= 0) continue;

                var fullPath = Environment.ExpandEnvironmentVariables(value.Substring(start, charCount));
                if (!fullPath.StartsWith(target.InstallLocation, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var node = new RegistryValueJunk(FirewallRulesKey, valueName, target, this);
                node.Confidence.Add(ConfidenceRecords.ExplicitConnection);
                results.Add(node);
            }

            return results;
        }

        private static Microsoft.Win32.RegistryKey GetFirewallRulesKey()
        {
            try
            {
                return RegistryTools.OpenRegistryKey(FirewallRulesKey);
            }
            catch (SystemException ex)
            {
                Debug.WriteLine("Failed to get firewall rule registry key: " + ex);
                return null;
            }
        }

        public override string CategoryName => "Junk_FirewallRule_GroupName";
    }
}