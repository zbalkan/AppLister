﻿using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Junk.Containers;
using Engine.Shared;
using Engine.Tools;

namespace Engine.Junk.Confidence
{
    internal static class ConfidenceGenerators
    {
        public static IEnumerable<ConfidenceRecord> GenerateConfidence(string itemName, ApplicationUninstallerEntry applicationUninstallerEntry) => GenerateConfidence(itemName, null, 0, applicationUninstallerEntry);

        internal static IEnumerable<ConfidenceRecord> GenerateConfidence(string itemName, string itemParentPath, int level, ApplicationUninstallerEntry applicationUninstallerEntry)
        {
            var matchResult = MatchStringToProductName(applicationUninstallerEntry, itemName);

            return GenerateConfidence(itemName, matchResult, itemParentPath, level, applicationUninstallerEntry);
        }

        internal static IEnumerable<ConfidenceRecord> GenerateConfidence(string itemName, int similarityToEntry, string itemParentPath, int level,
            ApplicationUninstallerEntry applicationUninstallerEntry)
        {
            if (similarityToEntry < 0)
            {
                yield break;
            }

            yield return similarityToEntry < 2
                ? ConfidenceRecords.ProductNamePerfectMatch
                : ConfidenceRecords.ProductNameDodgyMatch;

            // Base rating according to path depth. 0 is best
            yield return new ConfidenceRecord(2 - (Math.Abs(level) * 2));

            if (ItemNameEqualsCompanyName(applicationUninstallerEntry, itemName))
            {
                yield return ConfidenceRecords.ItemNameEqualsCompanyName;
            }

            if (level > 0 && applicationUninstallerEntry.Publisher
                    .IndexOf(PathTools.GetName(itemParentPath).Replace('_', ' '), StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                yield return ConfidenceRecords.CompanyNameMatch;
            }
        }

        // Check if name is the same as publisher, could be "Adobe AIR" getting matched to a folder "Adobe"
        internal static bool ItemNameEqualsCompanyName(ApplicationUninstallerEntry applicationUninstallerEntry, string itemName)
        {
            var publisher = applicationUninstallerEntry.Publisher.ToLowerInvariant();
            itemName = itemName.ToLowerInvariant();
            return !publisher.Equals(applicationUninstallerEntry.DisplayNameTrimmed.ToLowerInvariant()) && publisher.Contains(itemName);
        }

        /// <summary>
        ///     -1 if match failed, 0 if string matched perfectly, higher if match was worse
        /// </summary>
        internal static int MatchStringToProductName(ApplicationUninstallerEntry applicationUninstallerEntry, string str)
        {
            var productName = applicationUninstallerEntry.DisplayNameTrimmed.ToLowerInvariant();
            str = str.Replace('_', ' ').ToLowerInvariant().Trim();
            var lowestLength = Math.Min(productName.Length, str.Length);

            // Don't match short strings
            if (lowestLength <= 4)
            {
                return -1;
            }

            var result = Sift4.SimplestDistance(productName, str, 1);

            // Strings match perfectly
            if (result <= 1)
            {
                return result;
            }

            // If the product name contains company name, try trimming it and testing again
            var publisher = applicationUninstallerEntry.Publisher.ToLower();
            if (publisher.Length > 4 && productName.Contains(publisher))
            {
                var trimmedProductName = productName.Replace(publisher, "").Trim();
                if (trimmedProductName.Length <= 4)
                {
                    return -1;
                }

                var trimmedResult = Sift4.SimplestDistance(trimmedProductName, str, 1);

                if (trimmedResult <= 1)
                {
                    return trimmedResult;
                }
            }

            var dirToName = str.Contains(productName);
            var nameToDir = productName.Contains(str);

            if (dirToName || nameToDir)
            {
                return 2;
            }

            // Hard cut-off if the difference is more than a third of the checked name
            if (result < lowestLength / 3)
            {
                return result;
            }

            return -1;
        }

        /// <summary>
        ///     Check if there are any similar names that match DisplayNameTrimmed, and if there are
        ///     then add negative confidence to names other than the best match. This is to avoid
        ///     e.g. `AppX Extended` matching junk entries from `AppX`
        /// </summary>
        internal static void TestForSimilarNames(ApplicationUninstallerEntry thisUninstaller, IEnumerable<ApplicationUninstallerEntry> otherUninstallers,
            ICollection<KeyValuePair<JunkResultBase, string>> createdJunk)
        {
            if (createdJunk.Count == 0)
            {
                return;
            }

            var thisDisplayName = thisUninstaller.DisplayNameTrimmed;

            // Check if any of the other apps match any of the entries, as long as the app names
            // don't contain this app's name
            var otherFiltered = otherUninstallers.Where(x => x != thisUninstaller && !x.DisplayNameTrimmed.Contains(thisDisplayName)).ToList();
            var matchingWithOther = createdJunk.Where(x => otherFiltered.Any(y => y.DisplayNameTrimmed.Contains(x.Value)));

            if (createdJunk.Count >= 2)
            {
                // Check for folders with similar names like `AppX Extended` and `AppX` and give
                // confidence penalty to every one other than the best match
                matchingWithOther = matchingWithOther
                    .Concat(createdJunk
                        .Where(x => x.Value.Contains(thisDisplayName) || thisDisplayName.Contains(x.Value))
                        .OrderBy(x => Sift4.SimplestDistance(x.Value, thisDisplayName, 100))
                        .Skip(1))
                    .Distinct();
            }

            foreach (var sketchyJunk in matchingWithOther)
            {
                sketchyJunk.Key.Confidence.Add(ConfidenceRecords.UsedBySimilarNamedApp);
            }
        }
    }
}