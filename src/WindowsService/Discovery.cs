using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using WindowsService.Engine;
using WindowsService.Engine.Factory;
using WmiProvider;

namespace WindowsService
{
    internal class Discovery : IScanner
    {
        public List<Package> GetAll()
        {
            var apps = QueryApps();

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
                        Publisher = app.Publisher,
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
            return packages.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(p => p.Id).ToList();
        }

        private static string[] GetStartupEntries(ApplicationUninstallerEntry app)
        {
            if (!app.HasStartups)
            {
                return Array.Empty<string>();
            }
            return app.StartupEntries.Select(entry => entry.FullLongName).ToArray();
        }

        private IList<ApplicationUninstallerEntry> QueryApps()
        {
            ConfigureUninstallTools();

            var result = ApplicationUninstallerFactory.GetUninstallerEntries();

            Console.WriteLine($"Found {result.Count} applications.");
            return result;
        }

        private static void ConfigureUninstallTools()
        {
            UninstallToolsGlobalConfig.ScanWinUpdates = false;
            UninstallToolsGlobalConfig.QuietAutomatizationKillStuck = true;
            UninstallToolsGlobalConfig.QuietAutomatization = true;
            UninstallToolsGlobalConfig.UseQuietUninstallDaemon = true;
            UninstallToolsGlobalConfig.AutoDetectCustomProgramFiles = true;
            UninstallToolsGlobalConfig.EnableAppInfoCache = false;
        }

        private static string CleanString(string str) => Regex.Replace(str, "[^a-zA-Z0-9.()_]", string.Empty);
    }
}