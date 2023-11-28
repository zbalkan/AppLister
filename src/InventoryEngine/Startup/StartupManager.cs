using System;
using System.Collections.Generic;
using System.Linq;
using InventoryEngine.Factory;
using InventoryEngine.Tools;
using Microsoft.Win32.TaskScheduler;

namespace InventoryEngine.Startup
{
    internal static class StartupManager
    {
        static StartupManager()
        {
            Factories = new Dictionary<string, Func<IEnumerable<StartupEntryBase>>>
            {
                { "StartupEntries", StartupEntryFactory.GetStartupItems },
                { "Startup_ShortName_Task", TaskEntryFactory.GetTaskStartupEntries },
                { "Startup_Shortname_BrowserHelper", BrowserEntryFactory.GetBrowserHelpers },
                { "Startup_ShortName_Service", ServiceEntryFactory.GetServiceEntries }
            };
        }

        public static Dictionary<string, Func<IEnumerable<StartupEntryBase>>> Factories { get; }

        /// <summary>
        ///     Fill in the ApplicationUninstallerEntry.StartupEntries property with a list of
        ///     related StartupEntry objects. Old data is not cleared, only overwritten if necessary.
        /// </summary>
        /// <param name="allUninstallerEntries">
        ///     Uninstaller entries to assign to
        /// </param>
        /// <param name="allStartupEntries">
        ///     Startup entries to assign
        /// </param>
        public static void AssignStartupEntries(IEnumerable<ApplicationUninstallerEntry> allUninstallerEntries,
            IEnumerable<StartupEntryBase> allStartupEntries)
        {
            //if (startupEntries == null || uninstallers == null)
            //    return;

            var startups = allStartupEntries.ToList();
            var uninstallers = allUninstallerEntries.ToList();

            if (startups.Count == 0)
            {
                return;
            }

            foreach (var uninstaller in uninstallers)
            {
                var positives = startups.Where(startup =>
                {
                    if (startup.ProgramNameTrimmed?.Equals(uninstaller.DisplayNameTrimmed, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return true;
                    }

                    if (startup.CommandFilePath == null)
                    {
                        return false;
                    }

                    var instLoc = uninstaller.InstallLocation;
                    if (uninstaller.IsInstallLocationValid() && startup.CommandFilePath.StartsWith(instLoc, StringComparison.OrdinalIgnoreCase))
                    {
                        // Don't assign if there are any applications with more specific/deep
                        // install locations (same depth is fine)
                        var instLocations = uninstallers
                            .Where(e => e.IsInstallLocationValid())
                            .Select(e => e.InstallLocation)
                            .Where(i => startup.CommandFilePath.StartsWith(i, StringComparison.OrdinalIgnoreCase));

                        if (!instLocations.Any(i => i.Length > instLoc.Length))
                        {
                            return true;
                        }
                    }

                    var uninLoc = uninstaller.UninstallerLocation;
                    if (!string.IsNullOrEmpty(uninLoc) && startup.CommandFilePath.StartsWith(uninLoc, StringComparison.OrdinalIgnoreCase))
                    {
                        // Don't assign if there are any applications with more specific/deep
                        // install locations (same depth is fine)
                        var uninLocations = uninstallers
                            .Where(e => !string.IsNullOrEmpty(e.UninstallerLocation))
                            .Select(e => e.UninstallerLocation)
                            .Where(i => startup.CommandFilePath.StartsWith(i, StringComparison.OrdinalIgnoreCase));

                        if (!uninLocations.Any(i => i.Length > uninLoc.Length))
                        {
                            return true;
                        }
                    }

                    return false;
                }).ToList();

                if (positives.Count > 0)
                {
                    uninstaller.StartupEntries = positives;
                }
            }
        }
    }
}