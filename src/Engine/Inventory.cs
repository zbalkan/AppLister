﻿using System.Collections.Generic;
using Engine.Factory;
using Engine.Shared;

namespace Engine
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
            UninstallToolsGlobalConfig.AutoDetectCustomProgramFiles = true;

            // Scan application sources
            UninstallToolsGlobalConfig.ScanStoreApps = true;
            UninstallToolsGlobalConfig.ScanWinFeatures = true;
            UninstallToolsGlobalConfig.ScanWinUpdates = true;
            UninstallToolsGlobalConfig.ScanPreDefined = true;
            UninstallToolsGlobalConfig.ScanScoop = true;
            UninstallToolsGlobalConfig.ScanChocolatey = true;
            UninstallToolsGlobalConfig.ScanOculus = true;
            UninstallToolsGlobalConfig.ScanSteam = true;
        }
    }
}