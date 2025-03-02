using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Shared;
using Engine.Tools;
using WUApiLib;

namespace Engine.Factory
{
    internal class WindowsUpdateFactory : IIndependentUninstallerFactory
    {
        public string DisplayName => "Progress_AppStores_WinUpdates";

        private static string HelperPath { get; } = Path.Combine(UninstallToolsGlobalConfig.AssemblyLocation, "WinUpdateHelper.exe");

        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            var results = new List<ApplicationUninstallerEntry>();

            foreach (var update in GetUpdateList())
            {
                var entry = new ApplicationUninstallerEntry
                {
                    UninstallerKind = UninstallerType.WindowsUpdate,
                    IsUpdate = true,
                    RawPublisher = "Microsoft Corporation"
                };

                var updateIdentity = update.Identity;
                entry.RatingId = updateIdentity.UpdateID;
                if (GuidTools.TryExtractGuid(updateIdentity.UpdateID, out var result))
                    entry.BundleProviderKey = result;
                entry.DisplayVersion = ApplicationEntryTools.CleanupDisplayVersion(updateIdentity.RevisionNumber.ToString());
                entry.RawDisplayName = update.Title;
                entry.IsProtected = !update.IsUninstallable;
                entry.AboutUrl = update.SupportUrl;
                var date = update.LastDeploymentChangeTime;
                if (!DateTime.MinValue.Equals(date))
                    entry.InstallDate = date;
                entry.InstallDate = date;
                entry.UninstallString = $"\"{HelperPath}\" uninstall {entry.RatingId}";

                results.Add(entry);
            }

            return results;
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanWinUpdates;

        private static List<IUpdate> GetUpdateList()
        {
            var wuaSession = new UpdateSessionClass();
            var wuaSearcher = wuaSession.CreateUpdateSearcher();
            var wuaSearch = wuaSearcher.Search("IsInstalled=1 and IsPresent=1 and Type='Software'");
            return wuaSearch.Updates.OfType<IUpdate>().ToList();
        }
    }
}