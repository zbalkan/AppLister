using System;
using System.Linq;

namespace Engine.InfoAdders
{
    public class GenerateSteamHelperStrings : IMissingInfoAdder
    {
        public bool AlwaysRun { get; } = true;

        public string[] CanProduceValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.UninstallString)
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
        }
    }
}