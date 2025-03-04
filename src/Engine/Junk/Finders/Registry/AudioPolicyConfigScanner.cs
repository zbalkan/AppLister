using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Engine.Extensions;
using Engine.Junk.Confidence;
using Engine.Junk.Containers;
using Engine.Tools;

namespace Engine.Junk.Finders.Registry
{
    internal class AudioPolicyConfigScanner : IJunkCreator
    {
        public string CategoryName => "Junk_AudioPolicy_GroupName";

        private const string AudioPolicyConfigSubkey = @"Microsoft\Internet Explorer\LowRegistry\Audio\PolicyConfig\PropertyStore";

        public IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            var returnList = new List<IJunkResult>();

            if (string.IsNullOrEmpty(target.InstallLocation))
            {
                return returnList;
            }

            string pathRoot;

            try
            {
                pathRoot = Path.GetPathRoot(target.InstallLocation);
            }
            catch (SystemException ex)
            {
                Debug.WriteLine(ex);
                return returnList;
            }

            var unrootedLocation = pathRoot.Length >= 1
                ? target.InstallLocation.Replace(pathRoot, string.Empty)
                : target.InstallLocation;

            if (string.IsNullOrEmpty(unrootedLocation.Trim()))
            {
                return returnList;
            }

            using var key = RegistryTools.OpenRegistryKey(Path.Combine(SoftwareRegKeyScanner.KeyCu, AudioPolicyConfigSubkey));
            if (key == null)
            {
                return returnList;
            }

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey == null)
                {
                    continue;
                }

                var defVal = subKey.GetStringSafe(null);
                if (defVal?.Contains(unrootedLocation, StringComparison.InvariantCultureIgnoreCase) != true)
                {
                    continue;
                }

                var junk = new RegistryKeyJunk(subKey.Name, target, this);
                junk.Confidence.Add(ConfidenceRecords.ExplicitConnection);
                returnList.Add(junk);
            }

            return returnList;
        }

        public void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
        }
    }
}