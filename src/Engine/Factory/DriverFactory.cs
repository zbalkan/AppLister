using System.Collections.Generic;
using Engine.Shared;

namespace Engine.Factory
{
    internal partial class DriverFactory : IIndependentUninstallerFactory
    {
        public string DisplayName => "Progress_AppStores_Drivers";

        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            return MapTo(new NativeDriverStore().EnumeratePackages()).AsReadOnly();
        }

        private static List<ApplicationUninstallerEntry> MapTo(List<DriverStoreEntry> driverStoreEntries)
        {
            var entries = new List<ApplicationUninstallerEntry>(driverStoreEntries.Count);
            foreach (var entry in driverStoreEntries)
            {
                var uninstallerEntry = new ApplicationUninstallerEntry
                {
                    DisplayName = string.IsNullOrEmpty(entry.DeviceName) ? entry.DriverPublishedName : entry.DeviceName,
                    DisplayVersion = entry.DriverVersion?.ToString() ?? string.Empty,
                    InstallDate = entry.DriverDate,
                    IsOrphaned = false,
                    IsProtected = false,
                    IsRegistered = true,
                    IsUpdate = false,
                    IsWebBrowser = false,
                    IsDriver = true,
                    Is64Bit = entry.DriverArchitecture switch
                    {
                        NativeDriverStore.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64 => MachineType.X64,
                        NativeDriverStore.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL => MachineType.X86,
                        NativeDriverStore.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_IA64 => MachineType.Ia64,
                        _ => MachineType.Unknown,// Unknown architecture
                    },
                    Comment = $"Signed by {entry.DriverSignerName}",
                    RawPublisher = entry.DriverPkgProvider,
                    UninstallerKind = UninstallerType.Unknown, // No specific uninstaller type for drivers
                    InstallSource = entry.DriverInfPath
                };
                entries.Add(uninstallerEntry);
            }
            return entries;
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanDrivers;
    }
}