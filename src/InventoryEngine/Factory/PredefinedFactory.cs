using System.Collections.Generic;

namespace InventoryEngine.Factory
{
    /// <summary>
    /// Get uninstallers that were manually pre-defined.
    /// </summary>
    internal class PredefinedFactory : IIndependantUninstallerFactory
    {
        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries() => new List<ApplicationUninstallerEntry>();

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanPreDefined;
        public string DisplayName => "Progress_AppStores_Templates";
    }
}