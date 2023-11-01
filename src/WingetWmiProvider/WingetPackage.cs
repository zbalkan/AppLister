using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Text.Json;
using Microsoft.Win32;

namespace WingetWmiProvider
{
    [ManagementEntity(Name = "Win32_WingetPackage")]
    public class WingetPackage
    {
        private const string ServiceKeyPath = @"SOFTWARE\zb\WingetService";
        private const string PackageDataKey = "Packages";

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

        public WingetPackage(string id, string installedVersion, string name, bool isUpdateAvailable, string source, string[] availableVersions)
        {
            Id = id;
            InstalledVersion = installedVersion;
            Name = name;
            IsUpdateAvailable = isUpdateAvailable;
            Source = source;
            AvailableVersions = availableVersions;
        }

        [ManagementEnumerator]
        public static IEnumerable<WingetPackage> GetPackages()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(ServiceKeyPath))
            {
                var result = Array.Empty<WingetPackage>();
                if (key != null)
                {
                    var packageText = key.GetValue(PackageDataKey).ToString();

                    if (!string.IsNullOrEmpty(packageText))
                    {
                        result = JsonSerializer.Deserialize<WingetPackage[]>(packageText);
                    }
                }
                return result;
            }
        }
    }
}