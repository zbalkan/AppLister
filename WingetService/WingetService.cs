using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Win32;
using WingetShared;
using System.Management.Automation;
using System.Linq;

namespace WingetService
{
    public sealed class WingetService
    {
        private const string ServiceKeyPath = @"SOFTWARE\zb\WingetService";
        private const string PackageDataKey = "Packages";

        private List<WingetPackages> FetchWingetPackages()
        {
            var packages = new List<WingetPackages>();

            using (var ps = PowerShell.Create())
            {
                ps.AddCommand("Get-WinGetPackage");

                foreach (var result in ps.Invoke())
                {
                    var id = result.Properties["Id"].Value.ToString();
                    var installedVersion = result.Properties["InstalledVersion"].Value.ToString();
                    var name = result.Properties["ServiceKeyPath"].Value.ToString();
                    var isUpdateAvailable = bool.Parse(result!.Properties["IsUpdateAvailable"].Value.ToString()!);
                    var source = result.Properties["Source"].Value.ToString();
                    var availableVersions = ((object[])result.Properties["AvailableVersions"].Value).Select(o => o.ToString()).ToArray();

                    var package = new WingetPackages(id, installedVersion, name, isUpdateAvailable, source, availableVersions);
                    packages.Add(package);
                }
            }

            return packages;
        }

        public void FetchAndSaveWingetPackages()
        {
            var packages = FetchWingetPackages();

            var serialized = JsonSerializer.Serialize(packages);

            using var key = Registry.LocalMachine.OpenSubKey(ServiceKeyPath, true);
            key?.SetValue(PackageDataKey, serialized);
        }
    }
}