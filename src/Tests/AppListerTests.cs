using System.Linq;
using AppLister;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class AppListerTests
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
        public void WhenAppsNotPublished_DiscoveredApps_ShouldBeEmpty()
        {
            // Collect
            var publishedApps = _wmiScanner.GetAll();

            // Compare
            Assert.IsTrue(publishedApps.Count == 0);
        }

        [TestMethod]
        public void WhenAppsPublished_DiscoveredApps_ShouldBeEqual()
        {
            // Scan
            var discoveredApps = _discovery.GetAll();

            // Publish
            Publisher.Publish(discoveredApps);

            // Collect
            var publishedApps = _wmiScanner.GetAll();

            // Compare
            Assert.IsTrue(publishedApps.SequenceEqual(discoveredApps));
        }
    }
}