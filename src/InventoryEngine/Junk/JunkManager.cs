using System;
using System.Collections.Generic;
using System.Linq;
using InventoryEngine.Extensions;
using InventoryEngine.Junk.Containers;
using InventoryEngine.Junk.Finders;
using InventoryEngine.Shared;
using InventoryEngine.Tools;

namespace InventoryEngine.Junk
{
    internal static class JunkManager
    {
        public static IEnumerable<IJunkResult> FindJunk(IEnumerable<ApplicationUninstallerEntry> targets,
            ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
            var scanners = ReflectionTools.GetTypesImplementingBase<IJunkCreator>()
                .Attempt(Activator.CreateInstance)
                .Cast<IJunkCreator>()
                .ToList();

            foreach (var junkCreator in scanners)
            {
                junkCreator.Setup(allUninstallers);
            }

            var results = new List<IJunkResult>();
            var targetEntries = targets as IList<ApplicationUninstallerEntry> ?? targets.ToList();
            foreach (var junkCreator in scanners)
            {
                foreach (var target in targetEntries)
                {
                    try { results.AddRange(junkCreator.FindJunk(target)); }
                    catch (SystemException)
                    { // }
                    }
                }

                foreach (var target in targetEntries)
                {
                    results.AddRange(target.AdditionalJunk);
                }
            }
            return CleanUpResults(results);
        }

        public static IEnumerable<IJunkResult> FindProgramFilesJunk(
            ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
            var pfScanner = new ProgramFilesOrphans();
            pfScanner.Setup(allUninstallers);
            return CleanUpResults(pfScanner.FindAllJunk().ToList());
        }

        private static IEnumerable<IJunkResult> CleanUpResults(IEnumerable<IJunkResult> input)
        {
            var prohibitedLocations = GetProhibitedLocations();

            return RemoveDuplicates(input)
                .Where(x => JunkDoesNotPointToDirectories(x, prohibitedLocations))
                .Where(JunkDoesNotPointToSelf);
        }

        /// <summary>
        ///     Prevent suggesting removing special directories if the app for some reason was
        ///     installed into them or otherwise used them
        /// </summary>
        private static HashSet<string> GetProhibitedLocations()
        {
            var results = new HashSet<string>();

            void AddRange(IEnumerable<string> paths)
            {
                foreach (var path in paths
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Attempt(System.IO.Path.GetFullPath)
                    .Select(x => x.ToLowerInvariant()))
                {
                    results.Add(path);
                }
            }

            AddRange((Enum.GetValues(typeof(Csidl)) as Csidl[])
                .Attempt(WindowsTools.GetEnvironmentPath));

            var knownFolderstype = Type.GetType("Windows.Storage.KnownFolders, Microsoft.Windows.SDK.NET", false);

            // Might not be available on some systems
            if (knownFolderstype == null)
            {
                return results;
            }

            return results;
        }

        private static bool JunkDoesNotPointToDirectories(IJunkResult arg, HashSet<string> prohibitedDirs)
        {
            if (arg is FileSystemJunk fileSystemJunk)
            {
                return !prohibitedDirs.Contains(fileSystemJunk.Path.FullName.ToLowerInvariant());
            }

            return true;
        }

        /// <summary>
        ///     Make sure that the junk result doesn't point to this application.
        /// </summary>
        private static bool JunkDoesNotPointToSelf(IJunkResult x)
        {
            if (x is FileSystemJunk fileSystemJunk)
            {
                return fileSystemJunk.Path?.FullName.StartsWith(UninstallToolsGlobalConfig.AppLocation, StringComparison.OrdinalIgnoreCase) != true;
            }

            if (x is StartupJunkNode startupJunk)
            {
                return startupJunk.Entry?.CommandFilePath == null ||
                       !startupJunk.Entry.CommandFilePath.StartsWith(UninstallToolsGlobalConfig.AppLocation, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        /// <summary>
        ///     Merge duplicate junk entries and their confidence parts
        /// </summary>
        private static IEnumerable<IJunkResult> RemoveDuplicates(IEnumerable<IJunkResult> input)
        {
            foreach (var appGroup in input.GroupBy(x => x.Application))
            {
                foreach (var group in appGroup.GroupBy(x => PathTools.NormalizePath(x.GetDisplayName()).ToLowerInvariant()))
                {
                    IJunkResult firstJunkResult = null;
                    foreach (var junkResult in group)
                    {
                        if (firstJunkResult == null)
                        {
                            firstJunkResult = junkResult;
                        }
                        else
                        {
                            firstJunkResult.Confidence.AddRange(junkResult.Confidence.ConfidenceParts);
                        }
                    }

                    if (firstJunkResult != null)
                    {
                        yield return firstJunkResult;
                    }
                }
            }
        }
    }
}