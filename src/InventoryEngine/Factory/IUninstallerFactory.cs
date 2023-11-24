using System.Collections.Generic;

namespace InventoryEngine.Factory
{
    public interface IIndependantUninstallerFactory : IUninstallerFactory
    {
        bool IsEnabled();

        string DisplayName { get; }
    }

    public interface IUninstallerFactory
    {
        IList<ApplicationUninstallerEntry> GetUninstallerEntries();
    }
}