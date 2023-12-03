using System.Collections.Generic;

namespace InventoryEngine.Factory
{
    internal partial class WindowsFeatureFactory
    {
        private class WindowsFeatureInfo
        {
            public WindowsFeatureInfo()
            {
                CustomProperties = new List<KeyValuePair<string, string>>();
            }

            public string FeatureName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string RestartRequired { get; set; }
            public bool Enabled { get; set; }
            public IList<KeyValuePair<string, string>> CustomProperties { get; private set; }
        }
    }
}