using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Extensions;
using Engine.Junk.Confidence;
using Engine.Junk.Containers;
using Engine.Tools;

namespace Engine.Junk.Finders.Registry
{
    internal partial class RegisteredApplicationsFinder : JunkCreatorBase
    {
        public override string CategoryName => "Registered app capabilities";

        private const string RegAppsSubKeyPath = @"Software\RegisteredApplications";

        private static readonly string[] TargetRoots = { @"HKEY_CURRENT_USER\", @"HKEY_LOCAL_MACHINE\" };

        private List<RegAppEntry> _regAppsValueCache;

        public override IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            var isStoreApp = target.UninstallerKind == UninstallerType.StoreApp;
            if (isStoreApp && string.IsNullOrEmpty(target.RatingId))
            {
                throw new ArgumentException("StoreApp entry has no ID");
            }

            if (isStoreApp)
            {
                foreach (var regAppEntry in _regAppsValueCache)
                {
                    if (regAppEntry.AppName == null)
                    {
                        continue;
                    }

                    if (!string.Equals(regAppEntry.AppName, target.RatingId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Handle the value under RegisteredApps itself
                    var regAppResult = new RegistryValueJunk(regAppEntry.RegAppFullPath, regAppEntry.ValueName, target, this);
                    regAppResult.Confidence.Add(ConfidenceRecords.ExplicitConnection);
                    yield return regAppResult;

                    // Handle the key pointed at by the value
                    var appEntryKey = new RegistryKeyJunk(regAppEntry.AppKey, target, this);
                    appEntryKey.Confidence.Add(ConfidenceRecords.ExplicitConnection);
                    appEntryKey.Confidence.Add(ConfidenceRecords.IsStoreApp);
                    yield return appEntryKey;
                }
            }
            else
            {
                foreach (var regAppEntry in _regAppsValueCache)
                {
                    if (regAppEntry.AppName != null)
                    {
                        continue;
                    }

                    var generatedConfidence = ConfidenceGenerators.GenerateConfidence(regAppEntry.ValueName, target).ToList();

                    if (generatedConfidence.Count <= 0)
                    {
                        continue;
                    }

                    // Handle the value under RegisteredApps itself
                    var regAppResult = new RegistryValueJunk(regAppEntry.RegAppFullPath, regAppEntry.ValueName, target, this);
                    regAppResult.Confidence.AddRange(generatedConfidence);
                    yield return regAppResult;

                    // Handle the key pointed at by the value
                    const string capabilitiesSubkeyName = "\\Capabilities";
                    if (!regAppEntry.TargetSubKeyPath.EndsWith(capabilitiesSubkeyName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var capabilitiesKeyResult = new RegistryKeyJunk(regAppEntry.TargetFullPath, target, this);
                    capabilitiesKeyResult.Confidence.AddRange(generatedConfidence);
                    yield return capabilitiesKeyResult;

                    var ownerKey = regAppEntry.TargetFullPath.Substring(0,
                        regAppEntry.TargetFullPath.Length - capabilitiesSubkeyName.Length);

                    var subConfidence = ConfidenceGenerators.GenerateConfidence(Path.GetFileName(ownerKey),
                        target).ToList();
                    if (subConfidence.Count <= 0)
                    {
                        continue;
                    }

                    var subResult = new RegistryKeyJunk(ownerKey, target, this);
                    subResult.Confidence.AddRange(subConfidence);
                    yield return subResult;
                }
            }
        }

        public override void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
            base.Setup(allUninstallers);

            // Preload all values into a new cache
            _regAppsValueCache = new List<RegAppEntry>();

            foreach (var targetRootName in TargetRoots)
            {
                using var rootKey = RegistryTools.OpenRegistryKey(targetRootName);
                using var regAppsKey = rootKey.OpenSubKey(RegAppsSubKeyPath);
                if (regAppsKey == null)
                {
                    continue;
                }

                var names = regAppsKey.GetValueNames();

                var results = names.Attempt(n => new { name = n, value = regAppsKey.GetStringSafe(n) })
                    .Where(x => !string.IsNullOrEmpty(x.value))
                    .ToList();

                _regAppsValueCache.AddRange(results.Select(x => new RegAppEntry(x.name, targetRootName, x.value.Trim('\\', ' ', '"', '\''))));
            }
        }
    }
}