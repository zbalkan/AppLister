using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Engine.Extensions;
using Engine.Junk.Confidence;
using Engine.Junk.Containers;
using Engine.Shared;
using Engine.Tools;

namespace Engine.Junk.Finders.Drive
{
    internal class CommonDriveJunkScanner : JunkCreatorBase
    {
        public override string CategoryName => "Junk_Drive_GroupName";

        private static IEnumerable<DirectoryInfo> _foldersToCheck;

        public override IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target) => _foldersToCheck.SelectMany(x => FindJunkRecursively(x, target));

        public override void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
            base.Setup(allUninstallers);

            var allDirs = UninstallToolsGlobalConfig.JunkSearchDirs.Concat(UninstallToolsGlobalConfig.GetAllProgramFiles());
            var validDirs = allDirs.Attempt(dir =>
                            {
                                var dirinfo = new DirectoryInfo(dir);
                                return dirinfo.Exists ? dirinfo : null;
                            }).Where(x => x != null);
            _foldersToCheck = validDirs.DistinctBy(x => x.FullName.ToLowerInvariant()).ToList();
        }

        private static IEnumerable<ConfidenceRecord> GenerateConfidence(string itemName, string itemParentPath,
            ApplicationUninstallerEntry uninstaller, int level)
        {
            var baseOutput = ConfidenceGenerators.GenerateConfidence(itemName, itemParentPath, level, uninstaller).ToList();

            if (!baseOutput.Any(x => x.Change > 0))
            {
                return Enumerable.Empty<ConfidenceRecord>();
            }

            if (UninstallToolsGlobalConfig.QuestionableDirectoryNames.Contains(itemName, StringComparison.OrdinalIgnoreCase))
            {
                baseOutput.Add(ConfidenceRecords.QuestionableDirectoryName);
            }

            return baseOutput;
        }

        private IEnumerable<FileSystemJunk> FindJunkRecursively(DirectoryInfo directory, ApplicationUninstallerEntry uninstaller, int level = 0)
        {
            var added = new List<FileSystemJunk>();
            IEnumerable<FileSystemJunk> results = added;

            try
            {
                var dirs = directory.GetDirectories();

                foreach (var dir in dirs)
                {
                    if (UninstallToolsGlobalConfig.IsSystemDirectory(dir))
                    {
                        continue;
                    }

                    var generatedConfidence = GenerateConfidence(dir.FullName, directory.FullName, uninstaller, level).ToList();

                    FileSystemJunk newNode = null;
                    if (generatedConfidence.Any())
                    {
                        newNode = new FileSystemJunk(dir, uninstaller, this);
                        newNode.Confidence.AddRange(generatedConfidence);

                        if (CheckIfDirIsStillUsed(dir.FullName, GetOtherInstallLocations(uninstaller)))
                        {
                            newNode.Confidence.Add(ConfidenceRecords.DirectoryStillUsed);
                        }

                        added.Add(newNode);
                    }

                    if (level > 1)
                    {
                        continue;
                    }

                    var junkNodes = FindJunkRecursively(dir, uninstaller, level + 1).ToList();

                    // ReSharper disable once PossibleMultipleEnumeration
                    results = results.Concat(junkNodes);

                    if (newNode == null)
                    {
                        continue;
                    }

                    // Check if the directory will have nothing left after junk removal.
                    if (dir.GetFiles().Any())
                    {
                        continue;
                    }

                    var subDirs = dir.GetDirectories();
                    if (!subDirs.Any() || subDirs.All(d => junkNodes.Any(y => PathTools.PathsEqual(d.FullName, y.Path.FullName))))
                    {
                        newNode.Confidence.Add(ConfidenceRecords.AllSubdirsMatched);
                    }
                }

                ConfidenceGenerators.TestForSimilarNames(uninstaller, AllUninstallers, added.ConvertAll(x => new KeyValuePair<JunkResultBase, string>(x, Path.GetFileNameWithoutExtension(x.Path.FullName))));
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Debug.WriteLine(ex);
            }

            // ReSharper disable once PossibleMultipleEnumeration
            return results;
        }
    }
}