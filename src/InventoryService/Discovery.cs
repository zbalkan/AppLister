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
    ///     The class that utilizes scan engine to collect software packages
    /// </summary>
    public sealed class Discovery : IScanner
    {
        /// <summary>
        ///     Initiates the engine and scan in different stores
        /// </summary>
        /// <returns>
        ///     List of installed applications as a list of <see cref="Package" /> instances.
        /// </returns>
        public List<Package> GetAll()
        {
            var apps = Inventory.QueryApps();
            return MaptoPackage(apps);
        }

        private List<Package> MaptoPackage(IReadOnlyList<ApplicationUninstallerEntry> apps)
        {
            var packages = new List<Package>();
            foreach (var app in apps)
            {
                try
                {
                    packages.Add(new Package()
                    {
                        Id = ExtractId(app),
                        Name = app.DisplayNameTrimmed,
                        Version = app.DisplayVersion,
                        Publisher = app.PublisherTrimmed,
                        InstallDate = app.InstallDate,
                        IsSystemComponent = app.SystemComponent,
                        IsUninstallable = !app.IsProtected,
                        IsBrowser = app.IsWebBrowser,
                        Executables = app.GetSortedExecutables().ToArray(),
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
            return packages.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(p => p.Id).ToList();
        }

        private static string ExtractId(ApplicationUninstallerEntry app) => Regex
            .Replace(
            $"{string.Join("_", (new[] { app.DisplayNameTrimmed, app.DisplayVersion }).Where(str => !string.IsNullOrEmpty(str)))}"
            .ToLowerInvariant().Replace(" ", "_").Replace(".", "_").Replace("__", "_"),
            "[^a-zA-Z0-9.()_]", string.Empty);

        private bool CheckStoreApp(ApplicationUninstallerEntry app) => app.UninstallerKind == UninstallerType.StoreApp;

        private static string[] GetStartupEntries(ApplicationUninstallerEntry app)
        {
            if (!app.HasStartups)
            {
                return Array.Empty<string>();
            }
            return app.StartupEntries.Select(entry => entry.FullLongName).ToArray();
        }
    }
}