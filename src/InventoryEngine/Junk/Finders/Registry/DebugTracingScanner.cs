using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using InventoryEngine.Extensions;
using InventoryEngine.Junk.Confidence;
using InventoryEngine.Junk.Containers;

namespace InventoryEngine.Junk.Finders.Registry
{
    internal class DebugTracingScanner : IJunkCreator
    {
        public void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers)
        {
        }

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

            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Tracing", true);
                if (key != null && target.SortedExecutables != null)
                {
                    var exeNames = target.SortedExecutables.Select(Path.GetFileNameWithoutExtension).ToList();

                    foreach (var keyGroup in key.GetSubKeyNames()
                                 .Where(x => x.EndsWith("_RASAPI32") || x.EndsWith("_RASMANCS"))
                                 .Select(name => new { name, trimmed = name.Substring(0, name.LastIndexOf('_')) })
                                 .GroupBy(x => x.trimmed))
                    {
                        if (!exeNames.Contains(keyGroup.Key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        foreach (var keyName in keyGroup)
                        {
                            var junk = new RegistryKeyJunk(Path.Combine(key.Name, keyName.name), target, this);
                            junk.Confidence.Add(ConfidenceRecords.ExplicitConnection);
                            returnList.Add(junk);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is SecurityException || ex is IOException)
            {
                Debug.WriteLine(ex);
            }

            return returnList;
        }

        public string CategoryName => "Junk_DebugTracing_GroupName";
    }
}