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
        /// <returns>List of installed applciations as a list of <see cref="Package"/>instances.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1212:Remove redundant assignment.", Justification = "<Pending>")]
        public List<Package> GetAll()
        {
            var apps = Inventory.QueryApps();

            var packages = new List<Package>();
            foreach (var app in apps)
            {
                try
                {
                    packages.Add(new Package()
                    {
                        Id = CleanString($"{app.DisplayNameTrimmed}_{app.DisplayVersion}"
                            .ToLowerInvariant()
                            .Replace(" ", "_").Replace(".", "_").Replace("__", "_")),
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
            packages =  packages.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(p => p.Id).ToList();
            return packages;
        }

        private static string[] GetStartupEntries(ApplicationUninstallerEntry app)
        {
            if (!app.HasStartups)
            {
                return Array.Empty<string>();
            }
            return app.StartupEntries.Select(entry => entry.FullLongName).ToArray();
        }

        private static string CleanString(string str) => Regex.Replace(str, "[^a-zA-Z0-9.()_]", string.Empty);
    }
}