﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.InfoAdders
{
    public class UninstallerSearcher : IMissingInfoAdder
    {
        public bool AlwaysRun { get; } = false;

        public string[] CanProduceValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.UninstallString)
        };

        public InfoAdderPriority Priority { get; } = InfoAdderPriority.RunLast;

        public string[] RequiredValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.SortedExecutables)
        };

        public bool RequiresAllValues { get; } = true;

        private static readonly string[] UninstallerFilters = { "unins0", "uninstall", "uninst", "uninstaller" };

        public void AddMissingInformation(ApplicationUninstallerEntry target)
        {
            if (!string.IsNullOrEmpty(target.UninstallString) || target.SortedExecutables == null)
            {
                return;
            }

            // Attempt to find an uninstaller application
            foreach (var file in target.SortedExecutables.Concat(FindExtraExecutables(target.InstallLocation)))
            {
                string name;
                try
                {
                    name = Path.GetFileNameWithoutExtension(file);
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }
                }
                catch (ArgumentException)
                {
                    continue;
                }

                if (!UninstallerFilters.Any(filter =>
                        name.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase) ||
                        name.EndsWith(filter, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                target.UninstallString = file;
                return;
            }
        }

        private static IEnumerable<string> FindExtraExecutables(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return Enumerable.Empty<string>();
            }

            try
            {
                return Directory.GetFiles(directoryPath, "*.bat", SearchOption.TopDirectoryOnly);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            return Enumerable.Empty<string>();
        }
    }
}