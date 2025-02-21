namespace InventoryEngine.Factory
{
    internal partial class WindowsFeatureFactory
    {
        private class WindowsFeatureInfo
        {
            public string Description { get; set; }

            public string DisplayName { get; set; }

            public bool Enabled { get; set; }

            public string FeatureName { get; set; }
        }
    }
}