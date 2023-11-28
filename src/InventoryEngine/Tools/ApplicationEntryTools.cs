using System;
using System.Diagnostics;
using System.IO;
using InventoryEngine.Extensions;
using InventoryEngine.Shared;

namespace InventoryEngine.Tools
{
    internal static class ApplicationEntryTools
    {
        private const int MaxHoursForHighConfidence = 1;
        private const int MaxHoursForReasonableConfidence = 40;

        /// <summary>
        ///     Check how related are the two entries. Values above 0 mean there is good confidence
        /// </summary>
        internal static int AreEntriesRelated(ApplicationUninstallerEntry baseEntry, ApplicationUninstallerEntry otherEntry)
        {
            if (PathTools.PathsEqual(baseEntry.InstallLocation, otherEntry.InstallLocation))
            {
                return 100;
            }

            if (!string.IsNullOrEmpty(baseEntry.UninstallString))
            {
                if (PathTools.PathsEqual(baseEntry.UninstallString, otherEntry.UninstallString))
                {
                    return 100;
                }

                if (otherEntry.IsOrphaned
                    && !string.IsNullOrEmpty(otherEntry.InstallLocation)
                    && baseEntry.UninstallString.Contains(otherEntry.InstallLocation, StringComparison.InvariantCultureIgnoreCase))
                {
                    return 100;
                }
            }

            if (otherEntry.IsOrphaned
                && !string.IsNullOrEmpty(baseEntry.UninstallerLocation) && !string.IsNullOrEmpty(otherEntry.InstallLocation)
                && baseEntry.UninstallerLocation.StartsWith(otherEntry.InstallLocation, StringComparison.InvariantCultureIgnoreCase))
            {
                return 100;
            }

            var score = 0;

            if (!string.IsNullOrEmpty(baseEntry.RatingId) && !string.IsNullOrEmpty(otherEntry.RatingId))
            {
                AddScore(ref score, -5, 0, 10, baseEntry.RatingId == otherEntry.RatingId);
            }

            if (!string.IsNullOrEmpty(baseEntry.InstallLocation) && !string.IsNullOrEmpty(otherEntry.InstallLocation))
            {
                AddScore(ref score, -8, 0, -3, baseEntry.InstallLocation.Contains(otherEntry.InstallLocation,
                    StringComparison.InvariantCultureIgnoreCase));
            }

            if (baseEntry.Is64Bit != MachineType.Unknown && otherEntry.Is64Bit != MachineType.Unknown)
            {
                AddScore(ref score, -5, 0, 3, baseEntry.Is64Bit == otherEntry.Is64Bit);
            }
            else
            {
                AddScore(ref score, -5, 0, 3, null);
            }
            AddScore(ref score, -3, -1, 5, CompareDates(baseEntry.InstallDate, otherEntry.InstallDate));

            AddScore(ref score, -2, 0, 5, CompareStrings(baseEntry.DisplayVersion, otherEntry.DisplayVersion, true));
            AddScore(ref score, -5, 0, 5, CompareStrings(baseEntry.Publisher, otherEntry.Publisher));

            // Check if base entry was installed from inside other entry's install directory
            if (string.IsNullOrEmpty(baseEntry.InstallLocation) && !string.IsNullOrEmpty(baseEntry.InstallSource) &&
                !string.IsNullOrEmpty(otherEntry.InstallLocation) && otherEntry.InstallLocation.Length >= 5)
            {
                AddScore(ref score, 0, 0, 5, baseEntry.InstallSource.Contains(
                    otherEntry.InstallLocation, StringComparison.InvariantCultureIgnoreCase));
            }

            var nameSimilarity = CompareStrings(baseEntry.DisplayName, otherEntry.DisplayName);
            AddScore(ref score, -5, -2, 8, nameSimilarity);
            if (!nameSimilarity.HasValue || nameSimilarity == false)
            {
                var trimmedSimilarity = CompareStrings(baseEntry.DisplayNameTrimmed, otherEntry.DisplayNameTrimmed);
                // Don't risk it if names can't be compared at all
                //if (!trimmedSimilarity.HasValue && !nameSimilarity.HasValue) return false;
                AddScore(ref score, -5, -2, 8, trimmedSimilarity);
            }

            try
            {
                AddScore(ref score, -2, -2, 5, CompareStrings(baseEntry.DisplayNameTrimmed.Length < 5 ?
                    baseEntry.DisplayName : baseEntry.DisplayNameTrimmed, Path.GetFileName(otherEntry.InstallLocation)));
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
            //Debug.Assert(score <= 0);
            return score;
        }

        internal static string CleanupDisplayVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return string.Empty;
            }

            return version.Replace(", ", ".").Replace(". ", ".").Replace(",", ".").Replace(". ", ".").Trim();
        }

        internal static string ExtractDirectoryName(string uninstallerLocation)
        {
            if (!string.IsNullOrEmpty(uninstallerLocation))
            {
                try
                {
                    return Path.GetDirectoryName(uninstallerLocation);
                }
                catch (ArgumentException) { }
                catch (PathTooLongException) { }
            }

            return null;
        }

        internal static string ExtractFullFilename(string uninstallString)
        {
            if (!string.IsNullOrEmpty(uninstallString))
            {
                try
                {
                    var fileName = ProcessTools.SeparateArgsFromCommand(uninstallString).FileName;

                    Debug.Assert(!string.IsNullOrEmpty(fileName?.Trim()),
                        $"SeparateArgsFromCommand failed for {fileName}");

                    return fileName;
                }
                catch (ArgumentException) { }
                catch (FormatException) { }
            }

            return null;
        }

        /// <summary>
        ///     Check if path points to the windows installer program or to a .msi package
        /// </summary>
        /// <param name="path">
        /// </param>
        /// <returns>
        /// </returns>
        internal static bool PathPointsToMsiExec(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return path.ContainsAny(new[] { "msiexec ", "msiexec.exe" }, StringComparison.OrdinalIgnoreCase)
                   || path.EndsWith(".msi", StringComparison.OrdinalIgnoreCase);
        }

        private static void AddScore(ref int score, int failScore, int unsureScore, int successScore, bool? testResult)
        {
            if (!testResult.HasValue)
            {
                score += unsureScore;
            }
            else
            {
                score = testResult.Value ? score + successScore : score + failScore;
            }
        }

        /// <summary>
        ///     Check if dates are very close together, or if they differ by a few hours. Result is
        ///     null if the length of the difference can't be compared confidently.
        /// </summary>
        private static bool? CompareDates(DateTime a, DateTime b)
        {
            if (a.Equals(default) || b.Equals(default))
            {
                return null;
            }

            var totalHours = Math.Abs((a - b).TotalHours);

            if (totalHours > MaxHoursForReasonableConfidence)
            {
                return null;
            }

            if (totalHours <= MaxHoursForHighConfidence)
            {
                return true;
            }

            // One of the dates is lacking time part, so can't be compared
            if (a.TimeOfDay.TotalSeconds < 1 || b.TimeOfDay.TotalSeconds < 1)
            {
                return null;
            }

            return totalHours <= 1;
        }

        private static bool? CompareStrings(string a, string b, bool relaxMatchRequirement = false)
        {
            var lengthRequirement = !relaxMatchRequirement ? 5 : 4;
            if (a == null || a.Length < lengthRequirement || b == null || b.Length < lengthRequirement)
            {
                return null;
            }

            if (relaxMatchRequirement)
            {
                if (a.StartsWith(b, StringComparison.Ordinal) || b.StartsWith(a, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            var changesRequired = Sift4.SimplestDistance(a, b, 3);
            return changesRequired == 0 || changesRequired < a.Length / 6;
        }
    }
}