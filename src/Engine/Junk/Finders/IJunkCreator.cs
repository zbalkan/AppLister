using System.Collections.Generic;
using Engine.Junk.Containers;

namespace Engine.Junk.Finders
{
    public interface IJunkCreator
    {
        string CategoryName { get; }

        IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target);

        void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers);
    }
}