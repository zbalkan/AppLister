using System.Management.Instrumentation;

namespace WmiProvider
{
    [ManagementEntity(Name = "Win32_InventoryPackage")]
    public class Package
    {
        [ManagementKey]
        public string Id { get; set; }

        [ManagementProbe]
        public string Name { get; set; }

        [ManagementProbe]
        public string Version { get; set; }

        [ManagementProbe]
        public string Publisher { get; set; }
    }
}