using System.Collections.Generic;
using System.Linq;
using InventoryEngine.Junk.Confidence;
using InventoryEngine.Junk.Containers;
using InventoryEngine.Tools;

namespace InventoryEngine.Junk.Finders.Registry
{
    internal class InstallerFoldersScanner : JunkCreatorBase
    {
        public override IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            var installLocation = target.InstallLocation;
            if (string.IsNullOrEmpty(installLocation))
            {
                yield break;
            }

            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\Folders"))
            {
                if (key == null)
                {
                    yield break;
                }

                foreach (var valueName in key.GetValueNames())
                {
                    if (!PathTools.SubPathIsInsideBasePath(installLocation, valueName, true))
                    {
                        continue;
                    }

                    var node = new RegistryValueJunk(key.Name, valueName, target, this);
                    node.Confidence.Add(ConfidenceRecords.ExplicitConnection);

                    if (GetOtherInstallLocations(target).Any(x => PathTools.SubPathIsInsideBasePath(x, valueName, true)))
                    {
                        node.Confidence.Add(ConfidenceRecords.DirectoryStillUsed);
                    }

                    yield return node;
                }
            }
        }

        public override string CategoryName => "Junk_InstalledFolders_GroupName";
    }
}