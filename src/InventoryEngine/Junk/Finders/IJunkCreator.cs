using System.Collections.Generic;
using InventoryEngine.Junk.Containers;

namespace InventoryEngine.Junk.Finders
{
    public interface IJunkCreator
    {
        void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers);

        IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target);

        string CategoryName { get; }
    }
}