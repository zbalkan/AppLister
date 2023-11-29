using InventoryEngine.Tools;

namespace InventoryEngine.InfoAdders
{
    public class VersionCleaner : IMissingInfoAdder
    {
        public bool AlwaysRun { get; } = true;

        public string[] CanProduceValueNames { get; } = { };

        public InfoAdderPriority Priority { get; } = InfoAdderPriority.RunDeadLast;

        public string[] RequiredValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.DisplayVersion)
        };

        public bool RequiresAllValues { get; } = true;

        public void AddMissingInformation(ApplicationUninstallerEntry target)
        {
            if (string.IsNullOrEmpty(target.DisplayVersion))
            {
                return;
            }

            target.DisplayVersion = ApplicationEntryTools.CleanupDisplayVersion(target.DisplayVersion);
        }
    }
}