using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.TaskScheduler;
using WindowsService.Engine.Factory;
using WindowsService.Engine.Tools;

namespace WindowsService.Engine.Startup
{
    public static class StartupManager
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
                return;

            foreach (var uninstaller in uninstallers)
            {
                var positives = startups.Where(startup =>
                {
                    if (startup.ProgramNameTrimmed?.Equals(uninstaller.DisplayNameTrimmed, StringComparison.OrdinalIgnoreCase) == true)
                        return true;

                    if (startup.CommandFilePath == null)
                        return false;

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
                            return true;
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
                            return true;
                    }

                    return false;
                }).ToList();

                if (positives.Count > 0)
                    uninstaller.StartupEntries = positives;
            }
        }

        /// <summary>
        ///     Look for and return all of the startup items present on this computer.
        /// </summary>
        public static IEnumerable<StartupEntryBase> GetAllStartupItems()
        {
            return Factories.Values.Aggregate(Enumerable.Empty<StartupEntryBase>(),
                (result, factory) => result.Concat(factory()));
        }

        /// <summary>
        ///     Open locations of the startup entries in respective applications. (regedit, win
        ///     explorer, task scheduler)
        /// </summary>
        public static void OpenStartupEntryLocations(IEnumerable<StartupEntryBase> selection)
        {
            var startupEntryBases = selection as IList<StartupEntryBase> ?? selection.ToList();
            var regOpened = false;

            if (startupEntryBases.Any(x => x is TaskEntry))
                TaskService.Instance.StartSystemTaskSchedulerManager();

            var browserAddon = startupEntryBases.OfType<BrowserHelperEntry>().FirstOrDefault();
            if (browserAddon != null)
            {
                RegistryTools.OpenRegKeyInRegedit(browserAddon.FullLongName);
                regOpened = true;
            }

            foreach (var item in startupEntryBases)
            {
                if (item is StartupEntry s && s.IsRegKey)
                {
                    if (!regOpened)
                    {
                        RegistryTools.OpenRegKeyInRegedit(item.ParentLongName);
                        regOpened = true;
                    }
                }
                else if (!string.IsNullOrEmpty(item.FullLongName))
                    WindowsTools.OpenExplorerFocusedOnObject(item.FullLongName);
                else if (!string.IsNullOrEmpty(item.CommandFilePath))
                    WindowsTools.OpenExplorerFocusedOnObject(item.CommandFilePath);
            }
        }
    }
}