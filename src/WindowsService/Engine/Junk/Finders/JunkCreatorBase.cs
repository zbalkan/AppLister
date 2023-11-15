/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WindowsService.Engine.Junk.Confidence;
using WindowsService.Engine.Junk.Containers;
using WindowsService.Engine.Tools;

namespace WindowsService.Engine.Junk.Finders
{
    public abstract class JunkCreatorBase : IJunkCreator
    {
        public virtual void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
            AllUninstallers = allUninstallers;
        }

        protected ICollection<ApplicationUninstallerEntry> AllUninstallers { get; private set; }

        protected IEnumerable<ApplicationUninstallerEntry> GetOtherUninstallers(ApplicationUninstallerEntry exceptThis)
        {
            return AllUninstallers.Where(x => x != exceptThis);
        }

        protected IEnumerable<string> GetOtherInstallLocations(ApplicationUninstallerEntry target)
        {
            return GetOtherUninstallers(target).Select(x => x.InstallLocation).Where(x => !string.IsNullOrEmpty(x));
        }

        public abstract IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target);

        public abstract string CategoryName { get; }

        /// <summary>
        ///     Returns true if the dir is still used by other apps and can't be safely deleted.
        /// </summary>
        public static bool CheckIfDirIsStillUsed(string location, IEnumerable<string> otherInstallLocations)
        {
            return !string.IsNullOrEmpty(location) && otherInstallLocations.Any(x => x.TrimEnd('\\').StartsWith(location, StringComparison.InvariantCultureIgnoreCase));
        }

        private static readonly string FullWindowsDirectoryName = PathTools.GetWindowsDirectory().FullName;

        // TODO overhaul
        protected FileSystemJunk GetJunkNodeFromLocation(IEnumerable<string> otherInstallLocations, string directory, ApplicationUninstallerEntry app)
        {
            try
            {
                var dirInfo = new DirectoryInfo(directory);

                if (dirInfo.FullName.Contains(FullWindowsDirectoryName) || !dirInfo.Exists || dirInfo.Parent == null)
                    return null;

                var newNode = new FileSystemJunk(dirInfo, app, this);
                newNode.Confidence.Add(ConfidenceRecords.ExplicitConnection);

                if (CheckIfDirIsStillUsed(dirInfo.FullName, otherInstallLocations))
                    newNode.Confidence.Add(ConfidenceRecords.DirectoryStillUsed);

                return newNode;
            }
            catch
            {
                return null;
            }
        }
    }
}