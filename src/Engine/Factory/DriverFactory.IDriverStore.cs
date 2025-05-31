using System.Collections.Generic;

namespace Engine.Factory
{
    internal partial class DriverFactory
    {
        public interface IDriverStore
        {
            List<DriverStoreEntry> EnumeratePackages();
        }
    }
}