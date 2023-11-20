using Microsoft.VisualStudio.TestTools.UnitTesting;
using InventoryService;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class InventoryServiceTests
    {
        private Discovery discovery { get; set; }
        private WmiScanner wmiScanner { get; set; }

        [TestInitialize]
        public void Init()
        {
            discovery = new Discovery();
            wmiScanner = new WmiScanner();
        }

        [TestMethod]
        public void WhenPackagesNotPublished_DiscoveredPackages_ShouldBeEmpty()
        {
            // Collect
            var publishedPackages = wmiScanner.GetAll();

            // Compare
            Assert.IsTrue(publishedPackages.Count == 0);
        }

        [TestMethod]
        public void WhenPackagesPublished_DiscoveredPackages_ShouldBeEqual()
        {
            // Scan
            var discoveredPackages = discovery.GetAll();

            // Publish
            Publisher.Publish(discoveredPackages);

            // Collect
            var publishedPackages = wmiScanner.GetAll();

            // Compare
            Assert.IsTrue(publishedPackages.SequenceEqual(discoveredPackages));
        }
    }
}
