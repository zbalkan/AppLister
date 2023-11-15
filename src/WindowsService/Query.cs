using System;
using System.Collections.Generic;
using System.Linq;
using WindowsService.Engine;
using WindowsService.Engine.Factory;
using WmiProvider;

namespace WindowsService
{
    internal static class Query
    {
        public static IEnumerable<Package> GetAll()
        {
            var apps = QueryApps();

            var packages = new List<Package>();
            packages.AddRange(from app in apps
                              select new Package
                              {
                                  Id = $"{app.DisplayNameTrimmed}_{app.DisplayVersion}",
                                  Name = app.DisplayNameTrimmed,
                                  Version = app.DisplayVersion,
                                  Publisher = app.Publisher
                              });
            return packages;
        }

        private static IList<ApplicationUninstallerEntry> QueryApps()
        {
            ConfigureUninstallTools();

            var result = ApplicationUninstallerFactory.GetUninstallerEntries();

            Console.WriteLine("Found {0} applications.", result.Count);
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
    }
}