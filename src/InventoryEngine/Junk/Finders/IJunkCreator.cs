using System.Collections.Generic;
using InventoryEngine;
using InventoryEngine.Junk.Containers;

namespace UninstallTools.Junk.Finders
{
    public interface IJunkCreator
    {
        void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers);

        IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target);

        string CategoryName { get; }
    }
}