using System.Collections.Generic;
using InventoryEngine.Factory;

namespace InventoryEngine
{
    public static class Inventory
    {
        public static IList<ApplicationUninstallerEntry> QueryApps()
        {
            ConfigureUninstallTools();

            return ApplicationUninstallerFactory.GetUninstallerEntries();
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
