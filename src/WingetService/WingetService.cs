using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Win32;
using System.Management.Automation;
using Namotion.Reflection;
using System;
using Microsoft.Extensions.Logging;

namespace WingetService
{
    public sealed class WingetService
    {
        private readonly ILogger<WingetService> _logger;

        private const string ServiceKeyPath = @"SOFTWARE\zb\WingetService";
        private const string PackageDataKey = "Packages";

        public WingetService(ILogger<WingetService> logger)
        {
            _logger = logger;
        }

        private List<WingetPackages> FetchWingetPackages()
        {
            var packages = new List<WingetPackages>();

            using var ps = PowerShell.Create();
            ps.AddCommand("Get-WinGetPackage");
            var commandResult = ps.Invoke();

            if (commandResult == null)
            {
                return packages;
            }
            foreach (var item in commandResult)
            {
                var id = item.Properties["Id"].Value.ToString();
                var installedVersion = item.Properties["InstalledVersion"].Value.ToString();
                var name = item.Properties["Name"].Value.ToString();
                var isUpdateAvailable = (bool)item.Properties["IsUpdateAvailable"].Value;

                var source = string.Empty;
                if (item.HasProperty("Source"))
                {
                    source = item.Properties["Source"].Value.ToString();
                }

                var availableVersions = Array.Empty<string>();
                if (item.HasProperty("AvailableVersions"))
                {
                    availableVersions = item.Properties["AvailableVersions"].Value as string[];
                }

                var package = new WingetPackages()
                {
                    Id = id!,
                    InstalledVersion = installedVersion!,
                    Name = name!,
                    IsUpdateAvailable = isUpdateAvailable,
                    Source = source!,
                    AvailableVersions = availableVersions!
                };

                packages.Add(package);
            }

            return packages;
        }

        public void FetchAndSaveWingetPackages()
        {
            _logger.LogInformation("Fetching packages");
            var packages = FetchWingetPackages();

            var serialized = JsonSerializer.Serialize(packages);

            _logger.LogInformation("Saving packages to registry");
            using var key = Registry.LocalMachine.OpenSubKey(ServiceKeyPath, true);
            key?.SetValue(PackageDataKey, serialized, RegistryValueKind.String);
        }
    }
}