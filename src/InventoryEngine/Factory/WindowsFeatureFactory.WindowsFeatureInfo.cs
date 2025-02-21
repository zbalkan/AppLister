using System.Collections.Generic;

namespace InventoryEngine.Factory
{
    internal partial class WindowsFeatureFactory
    {
        private class WindowsFeatureInfo
        {
            public string FeatureName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; }
        }
    }
}