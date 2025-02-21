using System.Collections.Generic;
using System.IO;
using System.Linq;
using InventoryEngine.Extensions;
using InventoryEngine.Junk.Confidence;
using InventoryEngine.Junk.Containers;
using InventoryEngine.Tools;

namespace InventoryEngine.Junk.Finders.Registry
{
    internal class EventLogScanner : JunkCreatorBase
    {
        public override string CategoryName => "Junk_EventLog_GroupName";

        public override IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            if (string.IsNullOrEmpty(target.InstallLocation))
            {
                yield break;
            }

            var otherUninstallers = GetOtherUninstallers(target).ToList();

            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\EventLog\Application");
            if (key == null)
            {
                yield break;
            }

            var query = from name in key.GetSubKeyNames()
                        let m = ConfidenceGenerators.MatchStringToProductName(target, name)
                        where m >= 0 && m < 3
                        select name;

            foreach (var result in query)
            {
                using var subkey = key.OpenSubKey(result);
                var exePath = subkey?.GetStringSafe("EventMessageFile");
                if (string.IsNullOrEmpty(exePath) || !PathTools.SubPathIsInsideBasePath(target.InstallLocation, Path.GetDirectoryName(exePath), true))
                {
                    continue;
                }

                var node = new RegistryKeyJunk(subkey.Name, target, this);

                // Already matched names above
                node.Confidence.Add(ConfidenceRecords.ProductNamePerfectMatch);

                if (otherUninstallers.Any(x => PathTools.SubPathIsInsideBasePath(x.InstallLocation, Path.GetDirectoryName(exePath), true)))
                {
                    node.Confidence.Add(ConfidenceRecords.DirectoryStillUsed);
                }

                yield return node;
            }
        }
    }
}