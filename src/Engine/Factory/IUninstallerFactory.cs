using System.Collections.Generic;

namespace Engine.Factory
{
    public interface IUninstallerFactory
    {
        IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries();
    }
}