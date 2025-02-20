using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using InventoryEngine.Tools;

namespace InventoryEngine.Factory
{
    public class ScriptFactory : IIndependentUninstallerFactory
    {
        private static readonly PropertyInfo[] EntryProps;

        private static string HelperPath { get; } = Path.Combine(UninstallToolsGlobalConfig.AssemblyLocation, "ScriptHelper.exe");

        private static bool IsHelperAvailable() => File.Exists(HelperPath);

        static ScriptFactory()
        {
            EntryProps = typeof(ApplicationUninstallerEntry)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanWrite && p.PropertyType == typeof(string))
                .ToArray();
        }

        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            var results = new List<ApplicationUninstallerEntry>();
            if (!IsHelperAvailable())
            {
                return results;
            }

            var result = FactoryTools.StartHelperAndReadOutput(HelperPath, "list");

            if (string.IsNullOrEmpty(result))
            {
                return results;
            }

            var dataSets = FactoryTools.ExtractAppDataSetsFromHelperOutput(result);

            foreach (var dataSet in dataSets)
            {
                var entry = new ApplicationUninstallerEntry();

                // Automatically fill in any supplied static properties
                foreach (var entryProp in EntryProps)
                {
                    if (!dataSet.TryGetValue(entryProp.Name, out var item) || string.IsNullOrEmpty(item))
                    {
                        continue;
                    }

                    try
                    {
                        entryProp.SetValue(entry, item, null);
                    }
                    catch (SystemException ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }

                if (!entry.UninstallPossible && !entry.QuietUninstallPossible)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(entry.Publisher))
                {
                    entry.Publisher = "Script";
                }

                results.Add(entry);
            }

            return results.AsReadOnly();
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanPreDefined;

        public string DisplayName => "Progress_AppStores_Templates";
    }
}