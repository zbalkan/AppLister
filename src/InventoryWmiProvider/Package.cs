using System;
using System.Diagnostics;
using System.Management.Instrumentation;

namespace InventoryWmiProvider
{
    [ManagementEntity(Name = "CI_Application", Singleton = false)]
    [ManagementQualifier("Description", Value = "Software inventory: A read-only Win32_Product alternative")]
    [DebuggerDisplay("Package = {Id}")]
    public class Package : IEquatable<Package>
    {
        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Package architecture")]
        public string Architecture { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Package commments")]
        public string Comments { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Get the about URL")]
        public string AboutURL { get; set; }

        [ManagementKey]
        [ManagementQualifier("Description", Value = "Unique identifier: <Name>_<Version>")]
        public string Id { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Package installed date (DMTF datetime)")]
        public DateTime InstallDate { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "If the package is a browser")]
        public bool IsBrowser { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "A package is orphaned if there is not a related registry record for installation")]
        public bool IsOrphaned { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "If the package is installed and managed by Windows Store")]
        public bool IsStoreApp { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "If the package is a Windows system component")]
        public bool IsSystemComponent { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "If the package can be uninstalled")]
        public bool IsUninstallable { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "If the package is an update")]
        public bool IsUpdate { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Package name")]
        public string Name { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Package publisher information")]
        public string Publisher { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Get the startup entries of the application")]
        public string[] StartupEntries { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Package version")]
        public string Version { get; set; }

        public override bool Equals(object obj)
        {
            if (obj?.GetType() != typeof(Package))
            {
                return false;
            }
            return Id == ((Package)obj).Id;
        }

        public bool Equals(Package other) => Id == other?.Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}