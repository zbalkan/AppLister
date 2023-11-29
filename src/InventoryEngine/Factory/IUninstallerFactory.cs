using System.Collections.Generic;

namespace InventoryEngine.Factory
{
    public interface IUninstallerFactory
    {
        IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries();
    }
}