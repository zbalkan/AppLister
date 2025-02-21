using System.Collections.Generic;
using InventoryEngine.Junk.Containers;

namespace InventoryEngine.Junk.Finders
{
    public interface IJunkCreator
    {
        string CategoryName { get; }

        IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target);

        void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers);
    }
}