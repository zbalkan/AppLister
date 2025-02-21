using System.Collections.Generic;

namespace InventoryEngine.Junk.Finders.Registry
{
    internal partial class ComScanner
    {
        private sealed class ComEntry
        {
            public readonly string Guid;

            public readonly List<string> InterfaceNames = new List<string>();

            public string FullFilename;

            //https://docs.microsoft.com/en-us/windows/desktop/com/-progid--key
            public string ProgId;

            public string VersionIndependentProgId;

            public ComEntry(string guid)
            {
                Guid = guid;
            }
        }
    }
}