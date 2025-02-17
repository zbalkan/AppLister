using System;
using System.Collections.Generic;
using InventoryEngine.Junk.Confidence;
using InventoryEngine.Junk.Containers;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UninstallTools.Junk.Finders;
using InventoryEngine.Tools;
using InventoryEngine.Extensions;

namespace InventoryEngine.Factory
{
    internal class SteamFactory : IIndependentUninstallerFactory, IJunkCreator
    {
        private static bool GetSteamInfo(out string steamLocation)
        {
            steamLocation = null;

            if (File.Exists(SteamHelperPath))
            {
                var output = FactoryTools.StartHelperAndReadOutput(SteamHelperPath, "steam");
                if (!string.IsNullOrEmpty(output)
                    && !output.Contains("error", StringComparison.InvariantCultureIgnoreCase)
                    && Directory.Exists(output = output.Trim().TrimEnd('\\', '/')))
                {
                    steamLocation = output;
                    return true;
                }
            }

            return false;
        }

        internal static string SteamHelperPath { get; } = Path.Combine(UninstallToolsGlobalConfig.AssemblyLocation, "SteamHelper.exe");

        #region IIndependantUninstallerFactory

        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            var results = new List<ApplicationUninstallerEntry>();
            if (!GetSteamInfo(out var steamLocation))
            {
                return results;
            }

            var output = FactoryTools.StartHelperAndReadOutput(SteamHelperPath, "l /i");
            if (string.IsNullOrEmpty(output))
            {
                return results;
            }

            foreach (var data in FactoryTools.ExtractAppDataSetsFromHelperOutput(output))
            {
                if (!int.TryParse(data["AppId"], out var appId))
                {
                    continue;
                }

                var entry = new ApplicationUninstallerEntry
                {
                    DisplayName = data["Name"],
                    UninstallString = data["UninstallString"],
                    InstallLocation = data["InstallDirectory"],
                    UninstallerKind = UninstallerType.Steam,
                    IsValid = true,
                    IsOrphaned = true,
                    RatingId = "Steam App " + appId.ToString("G")
                };

                results.Add(entry);
            }

            results.Add(new ApplicationUninstallerEntry
            {
                AboutUrl = "http://steampowered.com/",
                InstallLocation = steamLocation,
                DisplayName = "Steam",
                UninstallerKind = UninstallerType.Nsis,
                UninstallString = Path.Combine(steamLocation, "uninstall.exe"),
                IsOrphaned = false,
                RatingId = "Steam",
                IsValid = File.Exists(Path.Combine(steamLocation, "uninstall.exe")),
                InstallDate = Directory.GetCreationTime(steamLocation),
                Publisher = "Valve Corporation",
            });

            return results.AsReadOnly();
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanSteam;
        public string DisplayName => "Progress_AppStores_Steam";

        #endregion

        #region IJunkCreator

        private static readonly string[] TempFolderNames = { "downloading", "shadercache", "temp" };
        public void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers) { }
        public IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            if (target.UninstallerKind != UninstallerType.Steam || string.IsNullOrEmpty(target.InstallLocation))
            {
                return Enumerable.Empty<IJunkResult>();
            }

            var results = new List<IJunkResult>();
            try
            {
                // Look for this appID in steam library's temporary folders (game is inside "common" folder, temp folders are next to that)
                var d = new DirectoryInfo(target.InstallLocation);
                if (d.Exists && d.Parent?.Name == "common" && d.Parent.Parent != null)
                {
                    var libraryDir = d.Parent.Parent.FullName;
                    Debug.Assert(target.RatingId.StartsWith("Steam App "));
                    var appIdStr = target.RatingId.Substring("Steam App ".Length);
                    foreach (var cacheFolderName in TempFolderNames)
                    {
                        var subpath = Path.Combine(libraryDir, cacheFolderName, appIdStr);
                        if (Directory.Exists(subpath))
                        {
                            var junk = new FileSystemJunk(new DirectoryInfo(subpath), target, this);
                            junk.Confidence.Add(ConfidenceRecords.ExplicitConnection);
                            junk.Confidence.Add(4);
                            results.Add(junk);
                        }
                    }
                }
                else
                {
                    Debug.Fail(target.InstallLocation + " does not point inside of a steam library's common folder");
                }
            }
            catch (SystemException e)
            {
                Debug.WriteLine(e);
            }
            return results;
        }

        public string CategoryName { get; } = "UninstallerType_Steam";

        #endregion
    }
}