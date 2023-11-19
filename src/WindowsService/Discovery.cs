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
                        Publisher = app.Publisher
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