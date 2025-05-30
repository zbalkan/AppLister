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
            return driverStoreEntries.ConvertAll(e => new ApplicationUninstallerEntry()
            {
                DisplayName = string.IsNullOrEmpty(e.DeviceName) ? e.DriverPublishedName : e.DeviceName,
                DisplayVersion = e.DriverVersion?.ToString() ?? string.Empty,
                InstallDate = e.DriverDate,
                IsOrphaned = false,
                IsProtected = false,
                IsRegistered = true,
                IsUpdate = false,
                IsWebBrowser = false,
                IsDriver = true,
                Is64Bit = e.DriverArchitecture switch
                {
                    NativeDriverStore.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64 => MachineType.X64,
                    NativeDriverStore.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL => MachineType.X86,
                    NativeDriverStore.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_IA64 => MachineType.Ia64,
                    _ => MachineType.Unknown, // Unknown architecture
                },
                Comment = $"Signed by {e.DriverSignerName}",
                RawPublisher = e.DriverPkgProvider,
                UninstallerKind = UninstallerType.Unknown, // No specific uninstaller type for drivers
                InstallSource = e.DriverInfPath
            });
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanDrivers;
    }
}