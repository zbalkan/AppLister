﻿using System.Linq;
using Engine.Tools;

namespace Engine.InfoAdders
{
    internal class WebBrowserMarker : IMissingInfoAdder
    {
        public bool AlwaysRun { get; }

        public string[] CanProduceValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.IsWebBrowser)
        };

        public InfoAdderPriority Priority { get; } = InfoAdderPriority.RunLast;

        public string[] RequiredValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.SortedExecutables)
        };

        public bool RequiresAllValues { get; } = true;

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
    }
}