using System.Linq;
using InventoryService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class InventoryServiceTests
    {
        private Discovery discovery;
        private WmiScanner wmiScanner;

        [TestInitialize]
        public void Init()
        {
            discovery = new Discovery();
            wmiScanner = new WmiScanner();
        }

        // ExceptionAdjustment: M:Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(System.Boolean) -T:Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException
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