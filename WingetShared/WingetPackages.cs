using System.Management.Instrumentation;

namespace WingetShared
{
    [ManagementEntity(Name = "Win32_WingetPackages")]
    public class WingetPackages
    {
        [ManagementKey]
        public string Id { get; set; }

        [ManagementProbe]
        public string InstalledVersion { get; set; }

        [ManagementProbe]
        public string Name { get; set; }

        [ManagementProbe]
        public bool IsUpdateAvailable { get; set; }

        [ManagementProbe]
        public string Source { get; set; }

        [ManagementProbe]
        public string[] AvailableVersions { get; set; }

        public WingetPackages(string id, string installedVersion, string name, bool isUpdateAvailable, string source, string[] availableVersions)
        {
            Id = id;
            InstalledVersion = installedVersion;
            Name = name;
            IsUpdateAvailable = isUpdateAvailable;
            Source = source;
            AvailableVersions = availableVersions;
        }
    }
}