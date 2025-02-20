using System;
using System.Linq;
using InventoryEngine.Factory;
using InventoryEngine.Shared;

namespace InventoryEngine.InfoAdders
{
    public class GenerateSteamHelperStrings : IMissingInfoAdder
    {
        public bool AlwaysRun { get; } = true;

        public string[] CanProduceValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.UninstallString),
            nameof(ApplicationUninstallerEntry.QuietUninstallString)
        };

        public InfoAdderPriority Priority { get; } = InfoAdderPriority.RunLast;

        public string[] RequiredValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.UninstallerKind),
            nameof(ApplicationUninstallerEntry.RatingId)
        };

        public bool RequiresAllValues { get; } = true;

        public void AddMissingInformation(ApplicationUninstallerEntry target)
        {
            if (target.UninstallerKind != UninstallerType.Steam)
            {
                return;
            }

            var appId = target.RatingId.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last();
            if (!int.TryParse(appId, out _))
            {
                return;
            }

            if (!target.UninstallPossible || UninstallToolsGlobalConfig.QuietAutomatization)
            {
                target.UninstallString = $"\"{SteamFactory.SteamHelperPath}\" uninstall {appId}";
            }

            if (UninstallToolsGlobalConfig.QuietAutomatization)
            {
                target.QuietUninstallString = $"\"{SteamFactory.SteamHelperPath}\" uninstall /silent {appId}";
            }
        }
    }
}