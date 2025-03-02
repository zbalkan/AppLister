using System;
using System.Diagnostics;
using System.Management.Instrumentation;

namespace InventoryWmiProvider
{
    [ManagementEntity(Name = "ZB_App", Singleton = false)]
    [ManagementQualifier("Description", Value = "Software inventory: A read-only Win32_Product alternative")]
    [DebuggerDisplay("App = {Id}")]
    public class App : IEquatable<App>
    {
        [ManagementProbe]
        [ManagementQualifier("Description", Value = "App architecture")]
        public string Architecture { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "App commments")]
        public string Comments { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Get the about URL")]
        public string AboutURL { get; set; }

        [ManagementKey]
        [ManagementQualifier("Description", Value = "Unique identifier: <Name>_<Version>")]
        public string Id { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "App installed date (DMTF datetime)")]
        public DateTime InstallDate { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "If the App is a browser")]
        public bool IsBrowser { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "A App is orphaned if there is not a related registry record for installation")]
        public bool IsOrphaned { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "If the App is installed and managed by Windows Store")]
        public bool IsStoreApp { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "If the App is a Windows system component")]
        public bool IsSystemComponent { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "If the App can be uninstalled")]
        public bool IsUninstallable { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "If the App is an update")]
        public bool IsUpdate { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "App name")]
        public string Name { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "App publisher information")]
        public string Publisher { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "Get the startup entries of the application")]
        public string[] StartupEntries { get; set; }

        [ManagementProbe]
        [ManagementQualifier("Description", Value = "App version")]
        public string Version { get; set; }

        public override bool Equals(object obj)
        {
            if (obj?.GetType() != typeof(App))
            {
                return false;
            }
            return Id == ((App)obj).Id;
        }

        public bool Equals(App other) => Id == other?.Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}