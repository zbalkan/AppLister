using System;
using System.Collections.Generic;
using System.IO;
using InventoryEngine.Tools;
using InventoryEngine.InfoAdders;
using InventoryEngine.Extensions;

namespace InventoryEngine.Factory
{
    public class OculusFactory : IIndependantUninstallerFactory
    {
        public string DisplayName => "Progress_AppStores_Oculus";
        private static string HelperPath { get; } = Path.Combine(UninstallToolsGlobalConfig.AssemblyLocation, "OculusHelper.exe");
        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            var results = new List<ApplicationUninstallerEntry>();
            if (!IsHelperAvailable())
            {
                return results;
            }

            var output = FactoryTools.StartHelperAndReadOutput(HelperPath, "/query");
            if (string.IsNullOrEmpty(output))
            {
                return results;
            }

            foreach (var data in FactoryTools.ExtractAppDataSetsFromHelperOutput(output))
            {
                if (!data.ContainsKey("CanonicalName"))
                {
                    continue;
                }

                var name = data["CanonicalName"];
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var uninstallStr = $"\"{HelperPath}\" /uninstall {name}";

                var entry = new ApplicationUninstallerEntry
                {
                    RatingId = name,
                    //RegistryKeyName = name,
                    UninstallString = uninstallStr,
                    QuietUninstallString = uninstallStr,
                    IsValid = true,
                    UninstallerKind = UninstallerType.Oculus,
                    InstallLocation = data["InstallLocation"],
                    DisplayVersion = ApplicationEntryTools.CleanupDisplayVersion(data["Version"]),
                    IsProtected = "true".Equals(data["IsCore"], StringComparison.OrdinalIgnoreCase),
                };

                Enrich(data, name, entry);

                results.Add(entry);
            }
            return results.AsReadOnly();
        }

        private static void Enrich(Dictionary<string, string> data, string name, ApplicationUninstallerEntry entry)
        {
            var executable = data["LaunchFile"];
            if (File.Exists(executable))
            {
                ExecutableAttributeExtractor.FillInformationFromFileAttribs(entry, executable, true);
            }

            if (Directory.Exists(entry.InstallLocation))
            {
                entry.InstallDate = Directory.GetCreationTime(entry.InstallLocation);
            }

            if (string.IsNullOrEmpty(entry.RawDisplayName))
            {
                entry.RawDisplayName = name.Replace('-', ' ').ToTitleCase();
            }
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanOculus;

        private static bool IsHelperAvailable() => File.Exists(HelperPath);
    }
}