using System.Collections.Generic;
using System.Linq;
using InventoryEngine.Junk.Containers;
using UninstallTools.Junk.Finders;

namespace InventoryEngine.Junk.Finders.Misc
{
    internal sealed class StartupJunk : IJunkCreator
    {
        public string CategoryName => "Junk_Startup_GroupName";

        public IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            if (target.StartupEntries == null)
            {
                return Enumerable.Empty<IJunkResult>();
            }

            return target.StartupEntries.Where(x => x.StillExists())
                .Select(x => (IJunkResult)new StartupJunkNode(x, target, this));
        }

        public void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
        }
    }
}