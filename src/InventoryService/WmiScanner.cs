using System.Collections.Generic;
using System.Linq;
using System.Management;
using InventoryWmiProvider;

namespace InventoryService
{
    public sealed class WmiScanner : IScanner
    {
        public List<App> GetAll()
        {
            var Apps = new List<App>();
            using (var searcher = new ManagementObjectSearcher("root\\cimv2", "SELECT * FROM ZB_App"))
            {
                foreach (var queryObj in searcher.Get().OfType<ManagementObject>())
                {
                    var App = new App
                    {
                        Id = queryObj["Id"] as string,
                        Name = queryObj["Name"] as string,
                        Version = queryObj["Version"] as string,
                        Publisher = queryObj["Publisher"] as string,
                        InstallDate = ManagementDateTimeConverter.ToDateTime(queryObj["InstallDate"] as string),
                        IsSystemComponent = (bool)queryObj["IsSystemComponent"],
                        IsUninstallable = (bool)queryObj["IsUninstallable"],
                        IsBrowser = (bool)queryObj["IsBrowser"],
                        AboutURL = queryObj["AboutURL"] as string,
                        IsUpdate = (bool)queryObj["IsUpdate"],
                        IsOrphaned = (bool)queryObj["IsOrphaned"],
                        StartupEntries = queryObj["StartupEntries"] as string[],
                        Architecture = queryObj["Architecture"] as string,
                        Comments = queryObj["Comments"] as string ?? string.Empty
                    };
                    Apps.Add(App);
                }
            }
            return Apps.OrderBy(p => p.Id).ToList();
        }
    }
}