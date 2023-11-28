using System.Collections.Generic;

namespace InventoryEngine.Startup
{
    internal interface IStartupDisable
    {
        IEnumerable<StartupEntry> AddDisableInfo(IList<StartupEntry> existingEntries);

        void Disable(StartupEntry startupEntry);

        void Enable(StartupEntry startupEntry);

        /// <summary>
        ///     Get backup store path for the link. The backup extension is appended as well. Works
        ///     only for links, file doesn't have to exist.
        /// </summary>
        string GetDisabledEntryPath(StartupEntry startupEntry);

        bool StillExists(StartupEntry startupEntry);
    }
}