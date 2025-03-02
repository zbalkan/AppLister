using System;
using System.IO;

namespace Engine.InfoAdders
{
    public class InstallDateAdder : IMissingInfoAdder
    {
        public bool AlwaysRun { get; } = false;

        public string[] CanProduceValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.InstallDate)
        };

        public InfoAdderPriority Priority { get; } = InfoAdderPriority.RunLast;

        public string[] RequiredValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.InstallLocation),
            nameof(ApplicationUninstallerEntry.UninstallerFullFilename)
        };

        public bool RequiresAllValues { get; } = false;

        public void AddMissingInformation(ApplicationUninstallerEntry target)
        {
            try
            {
                if (File.Exists(target.UninstallerFullFilename))
                {
                    target.InstallDate = File.GetCreationTime(target.UninstallerFullFilename);
                }
                else if (Directory.Exists(target.InstallLocation))
                {
                    target.InstallDate = Directory.GetCreationTime(target.InstallLocation);
                }
            }
            catch
            {
                target.InstallDate = DateTime.MinValue;
            }
        }
    }
}