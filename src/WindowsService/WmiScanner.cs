using System.Collections.Generic;
using System.Linq;
using System.Management;
using WmiProvider;

namespace WindowsService
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
                        Publisher = queryObj["Publisher"] as string
                    };
                    packages.Add(package);
                }
            }
            return packages.OrderBy(p => p.Id).ToList();
        }
    }
}