using System.Collections.Generic;
using System.Linq;
using System.Management;
using InventoryWmiProvider;

namespace InventoryService
{
    public class WmiScanner : IScanner
    {
        public List<Package> GetAll()
        {
            var packages = new List<Package>();
            using (var searcher = new ManagementObjectSearcher("root\\Inventory", "SELECT * FROM Win32_Package"))
            {
                foreach (var queryObj in searcher.Get().OfType<ManagementObject>())
                {
                    var package = new Package
                    {
                        Id = queryObj["Id"] as string,
                        Name = queryObj["Name"] as string,
                        Version = queryObj["Version"] as string,
                        Publisher = queryObj["Publisher"] as string,
                        InstallDate = ManagementDateTimeConverter.ToDateTime(queryObj["InstallDate"] as string),
                        IsSystemComponent = (bool)queryObj["IsSystemComponent"],
                        IsUninstallable = (bool)queryObj["IsUninstallable"],
                        IsBrowser = (bool)queryObj["IsBrowser"],
                        Executables = queryObj["Executables"] as string[],
                        IsUpdate = (bool)queryObj["IsUpdate"],
                        IsOrphaned = (bool)queryObj["IsOrphaned"],
                        StartupEntries = queryObj["StartupEntries"] as string[],
                        Architecture = queryObj["Architecture"] as string,
                        Comments = queryObj["Comments"] as string ?? string.Empty
                    };
                    packages.Add(package);
                }
            }
            return packages.OrderBy(p => p.Id).ToList();
        }
    }
}