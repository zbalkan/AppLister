using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using InventoryEngine;
using InventoryWmiProvider;

namespace InventoryService
{
    /// <summary>
    ///     The class that utilizes scan engine to collect installed Apps
    /// </summary>
    public sealed class Discovery : IScanner
    {
        /// <summary>
        ///     Initiates the engine and scan in different stores
        /// </summary>
        /// <returns>
        ///     List of installed applications as a list of <see cref="App" /> instances.
        /// </returns>
        public List<App> GetAll()
        {
            var apps = Inventory.QueryApps();
            return MaptoApp(apps);
        }

        private static string ExtractId(ApplicationUninstallerEntry app) => Regex
            .Replace(
            $"{string.Join("_", (new[] { app.DisplayNameTrimmed, app.DisplayVersion }).Where(str => !string.IsNullOrEmpty(str)))}"
            .ToLowerInvariant().Replace(" ", "_").Replace(".", "_").Replace("__", "_"),
            "[^a-zA-Z0-9.()_]", string.Empty);

        private static string[] GetStartupEntries(ApplicationUninstallerEntry app)
        {
            if (app.StartupEntries == null || !app.StartupEntries.Any())
            {
                return Array.Empty<string>();
            }
            return app.StartupEntries.Select(entry => entry.FullLongName).ToArray();
        }

        private bool CheckStoreApp(ApplicationUninstallerEntry app) => app.UninstallerKind == UninstallerType.StoreApp;

        private List<App> MaptoApp(IReadOnlyList<ApplicationUninstallerEntry> apps)
        {
            var Apps = new List<App>();
            foreach (var app in apps)
            {
                try
                {
                    Apps.Add(new App()
                    {
                        Id = ExtractId(app),
                        Name = app.DisplayNameTrimmed,
                        Version = app.DisplayVersion,
                        Publisher = app.Publisher,
                        InstallDate = app.InstallDate,
                        IsSystemComponent = app.SystemComponent,
                        IsUninstallable = !app.IsProtected,
                        IsBrowser = app.IsWebBrowser,
                        AboutURL = app.AboutUrl,
                        IsOrphaned = app.IsOrphaned,
                        IsUpdate = app.IsUpdate,
                        IsStoreApp = CheckStoreApp(app),
                        StartupEntries = GetStartupEntries(app),
                        Architecture = Enum.GetName(typeof(MachineType), app.Is64Bit),
                        Comments = app.Comment
                    }
                );
                }
                catch (NullReferenceException ex)
                {
                    // If possible, log
                    Trace.TraceError($"{ex.Message} @{app.DisplayNameTrimmed}");
                }
            }
            return Apps.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(p => p.Id).ToList();
        }
    }
}