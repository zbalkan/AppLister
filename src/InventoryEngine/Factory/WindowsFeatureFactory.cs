using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using InventoryEngine.Tools;

namespace InventoryEngine.Factory
{
    internal partial class WindowsFeatureFactory : IIndependentUninstallerFactory
    {
        public string DisplayName => "Progress_AppStores_WinFeatures";

        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            var results = new List<ApplicationUninstallerEntry>();

            Exception error = null;
            var t = new Thread(() =>
            {
                try
                {
                    results.AddRange(GetWindowsFeatures()
                        .Where(x => x.Enabled)
                        .Select(WindowsFeatureToUninstallerEntry));
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });
            t.Start();

            t.Join(TimeSpan.FromSeconds(40));

            if (error != null)
            {
                throw new IOException("Error while collecting Windows Features. If Windows Update is running wait until it finishes and try again. If the error persists try restarting your computer. In case nothing helps, read the KB957310 article.", error);
            }

            if (t.IsAlive)
            {
                throw new TimeoutException("WMI query has hung while collecting Windows Features, try restarting your computer. If the error persists read the KB957310 article.");
            }

            return results;
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanWinFeatures;

        private static string GetDismUninstallString(string featureName, bool silent) => string.Format("Dism.exe /norestart {1}/online /disable-feature /featurename=\"{0}\"",
                        featureName, silent ? "/quiet " : string.Empty);

        /// <summary>
        ///     Get information about enabled and disabled windows features. Works on Windows 7 and newer.
        /// </summary>
        private static IEnumerable<WindowsFeatureInfo> GetWindowsFeatures()
        {
            var features = new List<WindowsFeatureInfo>();

            var searcher = new ManagementObjectSearcher(new ManagementScope(),
                new ObjectQuery("select * from Win32_OptionalFeature"),
                new EnumerationOptions(null, TimeSpan.FromSeconds(35), 100, false, false, false, false, false, false, false));
            using (var moc = searcher.Get())
            {
                var items = moc.Cast<ManagementObject>().ToList();
                foreach (var managementObject in items)
                {
                    var featureInfo = new WindowsFeatureInfo();
                    foreach (var property in managementObject.Properties)
                    {
                        if (property.Name == "Caption")
                        {
                            featureInfo.DisplayName = property.Value.ToString();
                        }
                        else if (property.Name == "InstallState")
                        {
                            var status = (uint)property.Value;
                            if (status == 2)
                            {
                                featureInfo.Enabled = false;
                            }
                            else if (status == 1)
                            {
                                featureInfo.Enabled = true;
                            }
                            else
                            {
                                featureInfo.FeatureName = null;
                                break;
                            }
                        }
                        else if (property.Name == "Name")
                        {
                            featureInfo.FeatureName = property.Value.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(featureInfo.FeatureName))
                    {
                        continue;
                    }

                    features.Add(featureInfo);
                }
            }

            return features;
        }

        private static ApplicationUninstallerEntry WindowsFeatureToUninstallerEntry(WindowsFeatureInfo info)
        {
            var displayName = !string.IsNullOrEmpty(info.DisplayName) ? info.DisplayName : info.FeatureName;

            return new ApplicationUninstallerEntry
            {
                RawDisplayName = displayName,
                Comment = info.Description,
                UninstallString = GetDismUninstallString(info.FeatureName, false),
                QuietUninstallString = GetDismUninstallString(info.FeatureName, true),
                UninstallerKind = UninstallerType.WindowsFeature,
                Publisher = "Microsoft Corporation",
                IsValid = true,
                Is64Bit = ProcessTools.Is64BitProcess ? MachineType.X64 : MachineType.X86,
                RatingId = "WindowsFeature_" + info.FeatureName
            };
        }
    }
}