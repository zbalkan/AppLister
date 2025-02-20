using System.Linq;
using InventoryService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class InventoryServiceTests
    {
        private Discovery _discovery;
        private WmiScanner _wmiScanner;

        [TestInitialize]
        public void Init()
        {
            _discovery = new Discovery();
            _wmiScanner = new WmiScanner();
        }

        // ExceptionAdjustment: M:Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(System.Boolean) -T:Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException
        [TestMethod]
        public void WhenPackagesNotPublished_DiscoveredPackages_ShouldBeEmpty()
        {
            // Collect
            var publishedPackages = _wmiScanner.GetAll();

            // Compare
            Assert.IsTrue(publishedPackages.Count == 0);
        }

        [TestMethod]
        public void WhenPackagesPublished_DiscoveredPackages_ShouldBeEqual()
        {
            // Scan
            var discoveredPackages = _discovery.GetAll();

            // Publish
            Publisher.Publish(discoveredPackages);

            // Collect
            var publishedPackages = _wmiScanner.GetAll();

            // Compare
            Assert.IsTrue(publishedPackages.SequenceEqual(discoveredPackages));
        }
    }
}