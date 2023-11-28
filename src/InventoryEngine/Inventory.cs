using System.Collections.Generic;
using InventoryEngine.Factory;

namespace InventoryEngine
{
    public static class Inventory
    {
        public static IReadOnlyList<ApplicationUninstallerEntry> QueryApps()
        {
            ConfigureUninstallTools();

            return ApplicationUninstallerFactory.GetUninstallerEntries();
        }

        private static void ConfigureUninstallTools()
        {
            UninstallToolsGlobalConfig.QuietAutomatizationKillStuck = true;
            UninstallToolsGlobalConfig.QuietAutomatization = true;
            UninstallToolsGlobalConfig.UseQuietUninstallDaemon = true;
            UninstallToolsGlobalConfig.AutoDetectCustomProgramFiles = true;
            UninstallToolsGlobalConfig.EnableAppInfoCache = false;

            // Scan application sources
            UninstallToolsGlobalConfig.ScanStoreApps = true;
            UninstallToolsGlobalConfig.ScanWinFeatures = true;
            UninstallToolsGlobalConfig.ScanWinUpdates = false;
            UninstallToolsGlobalConfig.ScanPreDefined = true;
            UninstallToolsGlobalConfig.ScanScoop = true;
            UninstallToolsGlobalConfig.ScanChocolatey = true;
            UninstallToolsGlobalConfig.ScanOculus = true;
            UninstallToolsGlobalConfig.ScanSteam = true;
        }
    }
}