using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Engine.Extensions;
using Engine.Junk.Confidence;
using Engine.Junk.Containers;
using Engine.Tools;

namespace Engine.Junk.Finders.Registry
{
    internal class UninstallerKeySearcher : IJunkCreator
    {
        public string CategoryName => "Junk_UninstallerKey_GroupName";

        private static readonly IEnumerable<string> InstallerSubkeyPaths;

        /// <summary>
        ///     parent key path, upgrade code(key name)
        /// </summary>
        private IEnumerable<KeyValuePair<string, string>> _targetKeys;

        static UninstallerKeySearcher()
        {
            InstallerSubkeyPaths = new[]
            {
                @"SOFTWARE\Classes\Installer\Products",
                @"SOFTWARE\Classes\Installer\Features",
                @"SOFTWARE\Classes\Installer\Patches"
            };

            try
            {
                var currentUserId = WindowsTools.GetUserSid().Value;
                if (string.IsNullOrEmpty(currentUserId) || currentUserId.Length <= 9)
                {
                    return;
                }

                var currentUserInstallerDataPath = Path.Combine(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData",
                    currentUserId);

                InstallerSubkeyPaths = InstallerSubkeyPaths.Concat(new[]
                {
                    Path.Combine(currentUserInstallerDataPath, "Products"),
                    Path.Combine(currentUserInstallerDataPath, "Patches"),
                    Path.Combine(currentUserInstallerDataPath, "Components")
                });
            }
            catch (SystemException ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            if (RegistryTools.RegKeyStillExists(target.RegistryPath))
            {
                var regKeyNode = new RegistryKeyJunk(target.RegistryPath, target, this);
                regKeyNode.Confidence.Add(ConfidenceRecords.IsUninstallerRegistryKey);
                yield return regKeyNode;
            }

            if (target.UninstallerKind != UninstallerType.Msiexec || target.BundleProviderKey != Guid.Empty)
            {
                yield break;
            }

            var upgradeKey = MsiTools.ConvertBetweenUpgradeAndProductCode(target.BundleProviderKey).ToString("N");

            var matchedKeyPaths = _targetKeys
                .Where(x => x.Value.Equals(upgradeKey, StringComparison.OrdinalIgnoreCase));

            foreach (var keyPath in matchedKeyPaths)
            {
                var fullKeyPath = Path.Combine(keyPath.Key, keyPath.Value);
                var result = new RegistryKeyJunk(fullKeyPath, target, this);
                result.Confidence.Add(ConfidenceRecords.ExplicitConnection);
                yield return result;
            }
        }

        public void Setup(ICollection<ApplicationUninstallerEntry> allUninstallers) => _targetKeys = InstallerSubkeyPaths
                        .Using(x => Microsoft.Win32.Registry.LocalMachine.OpenSubKey(x))
                .Where(k => k != null)
                .SelectMany(k =>
                {
                    var parentPath = k.Name;
                    return k.GetSubKeyNames().Select(n => new KeyValuePair<string, string>(parentPath, n));
                }).ToList();
    }
}