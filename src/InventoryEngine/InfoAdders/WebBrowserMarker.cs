using System.Linq;
using InventoryEngine.Tools;

namespace InventoryEngine.InfoAdders
{
    internal class WebBrowserMarker : IMissingInfoAdder
    {
        private static readonly string[] BrowserExecutables = WindowsTools.GetInstalledWebBrowsers().ToArray();

        public void AddMissingInformation(ApplicationUninstallerEntry target)
        {
            if (target == null || target.SortedExecutables.Length == 0)
            {
                return;
            }

            target.IsWebBrowser = target.SortedExecutables.Any(
                x => BrowserExecutables.Any(
                    y => PathTools.PathsEqual(x, y)));
        }

        public string[] RequiredValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.SortedExecutables)
        };

        public string[] CanProduceValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.IsWebBrowser)
        };

        public bool RequiresAllValues { get; } = true;
        public bool AlwaysRun { get; }
        public InfoAdderPriority Priority { get; } = InfoAdderPriority.RunLast;
    }
}