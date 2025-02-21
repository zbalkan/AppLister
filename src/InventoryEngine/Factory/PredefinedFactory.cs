﻿using System.Collections.Generic;
using InventoryEngine.Shared;

namespace InventoryEngine.Factory
{
    /// <summary>
    ///     Get uninstallers that were manually pre-defined.
    /// </summary>
    internal class PredefinedFactory : IIndependentUninstallerFactory
    {
        public string DisplayName => "Progress_AppStores_Templates";

        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries() => new List<ApplicationUninstallerEntry>();

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanPreDefined;
    }
}