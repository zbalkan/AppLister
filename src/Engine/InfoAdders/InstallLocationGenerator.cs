namespace Engine.InfoAdders
{
    public class InstallLocationGenerator : IMissingInfoAdder
    {
        public bool AlwaysRun { get; } = false;

        public string[] CanProduceValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.InstallLocation)
        };

        public InfoAdderPriority Priority { get; } = InfoAdderPriority.RunFirst;

        public string[] RequiredValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.UninstallerKind),
            nameof(ApplicationUninstallerEntry.UninstallerLocation)
        };

        public bool RequiresAllValues { get; } = true;

        public void AddMissingInformation(ApplicationUninstallerEntry target)
        {
            if (target.UninstallerKind != UninstallerType.Nsis && target.UninstallerKind != UninstallerType.InnoSetup)
            {
                return;
            }

            if (!string.IsNullOrEmpty(target.UninstallerLocation))
            {
                target.InstallLocation = target.UninstallerLocation;
            }
        }
    }
}