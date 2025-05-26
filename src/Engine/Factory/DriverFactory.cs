using System.Collections.Generic;
using Engine.Shared;

namespace Engine.Factory
{
    internal partial class DriverFactory : IIndependentUninstallerFactory
    {
        public string DisplayName => "Progress_AppStores_Drivers";

        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            var driverStoreEntries = GetDriverStoreEntries();
            var entries = new List<ApplicationUninstallerEntry>(driverStoreEntries.Count);
            foreach (var entry in driverStoreEntries)
            {
                var uninstallerEntry = new ApplicationUninstallerEntry();
                uninstallerEntry.DisplayName = string.IsNullOrEmpty(entry.DeviceName) ? entry.DriverPublishedName : entry.DeviceName;
                uninstallerEntry.DisplayVersion = entry.DriverVersion?.ToString() ?? string.Empty;
                uninstallerEntry.InstallDate = entry.DriverDate;
                uninstallerEntry.IsOrphaned = false;
                uninstallerEntry.IsProtected = false;
                uninstallerEntry.IsRegistered = true;
                uninstallerEntry.IsUpdate = false;
                uninstallerEntry.IsWebBrowser = false;
                uninstallerEntry.IsDriver = true;
                uninstallerEntry.Is64Bit = entry.DriverArchitecture switch
                {
                    NativeDriverStore.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64 => MachineType.X64,
                    NativeDriverStore.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL => MachineType.X86,
                    NativeDriverStore.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_IA64 => MachineType.Ia64,
                    _ => MachineType.Unknown,// Unknown architecture
                };
                uninstallerEntry.Comment = $"Signed by {entry.DriverSignerName}";
                uninstallerEntry.RawPublisher = entry.DriverPkgProvider;
                uninstallerEntry.UninstallerKind = UninstallerType.Unknown; // No specific uninstaller type for drivers
                uninstallerEntry.InstallSource = entry.DriverInfPath;
                entries.Add(uninstallerEntry);
            }

            return entries.AsReadOnly();
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanDrivers;

        private List<DriverStoreEntry> GetDriverStoreEntries()
        {
            var driverStore = new NativeDriverStore();
            return driverStore.EnumeratePackages();
        }
    }
}