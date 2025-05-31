using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Engine.Factory
{
    internal partial class DriverFactory
    {
        public partial class NativeDriverStore : IDriverStore
        {
            private const int LOCALE_NAME_MAX_LENGTH = 85;

            // Define other methods and classes here
            private const int MAX_PATH = 260;

            public List<DriverStoreEntry> EnumeratePackages()
            {
                var ptr = NativeMethods.DriverStoreOpen(null, null, 0, IntPtr.Zero);
                if (ptr == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                var driverStoreEntries = new List<DriverStoreEntry>();

                try
                {
                    {
                        var handle = GCHandle.Alloc(driverStoreEntries);
                        try
                        {
                            NativeMethods.DriverStoreEnum(
                                ptr,
                                DriverStoreEnumFlags.OemOnly,
                                EnumDriverPackages,
                                GCHandle.ToIntPtr(handle));
                        }
                        finally
                        {
                            handle.Free();
                        }
                    }
                }
                finally
                {
                    NativeMethods.DriverStoreClose(ptr);
                }

                return ConfigManager.FillDeviceInfo(driverStoreEntries);
            }

            internal static bool EnumDriverPackages(
                IntPtr driverStoreHandle,
                string driverStoreFilename,
                DriverPackageInfo pDriverPackageInfo,
                IntPtr lParam)
            {
                var driverStoreEntries = (List<DriverStoreEntry>)GCHandle.FromIntPtr(lParam).Target;

                if (string.IsNullOrEmpty(pDriverPackageInfo.PublishedInfName))
                {
                    return true; // Skip invalid entries
                }

                driverStoreEntries.Add(new DriverStoreEntry
                {
                    DriverClass = ConfigManager.GetClassProperty<string>(GetObjectPropertyInfo<Guid>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_ClassGuid), DeviceHelper.DEVPKEY_DeviceClass_Name),
                    DriverInfPath = driverStoreFilename,
                    DriverPublishedName = pDriverPackageInfo.PublishedInfName,
                    DriverPkgProvider = GetObjectPropertyInfo<string>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_ProviderName),
                    DriverSignerName = GetObjectPropertyInfo<string>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_SignerName),
                    DriverDate = GetObjectPropertyInfo<DateTime>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_DriverDate),
                    DriverVersion = GetObjectPropertyInfo<Version>(driverStoreHandle, driverStoreFilename, DeviceHelper.DEVPKEY_DriverPackage_DriverVersion),
                    DriverArchitecture = GetProcessorArchitecture(driverStoreHandle)
                });

                return true;
            }

            internal static T GetObjectPropertyInfo<T>(
                IntPtr driverStoreHandle,
                string objectName,
                DevPropKey propertyKey,
                DriverStoreObjectType objectType = DriverStoreObjectType.DriverPackage)
            {
                // First pass: request size
                var ok = NativeMethods.DriverStoreGetObjectProperty(
                    driverStoreHandle,
                    objectType,
                    objectName,
                    ref propertyKey,
                    out _,
                    IntPtr.Zero,
                    0,
                    out var propertySize,
                    DriverStoreSetObjectPropertyFlags.None);

                var lastError = Marshal.GetLastWin32Error();

                // If call unexpectedly succeeded but size is zero, property exists but is empty
                if (ok && propertySize == 0)
                {
                    return default;
                }

                // If buffer was too small, ERROR_INSUFFICIENT_BUFFER (122) is returned
                const int ERROR_INSUFFICIENT_BUFFER = 122;
                if (!ok && lastError != ERROR_INSUFFICIENT_BUFFER)
                {
                    // No such property or another error; treat missing property as default
                    return default;
                }

                // Second pass: allocate buffer of exact size and fetch
                if (propertySize > int.MaxValue)
                {
                    throw new OverflowException($"Property size {propertySize} exceeds maximum buffer length.");
                }
                var bufferSize = (int)propertySize; // Ensure buffer size is within int range
                var buf = Marshal.AllocHGlobal(bufferSize);

                try
                {
                    ok = NativeMethods.DriverStoreGetObjectProperty(
                        driverStoreHandle,
                        objectType,
                        objectName,
                        ref propertyKey,
                        out var propertyType,
                        buf,
                        bufferSize,
                        out propertySize,
                        DriverStoreSetObjectPropertyFlags.None);

                    if (!ok)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    return DeviceHelper.ConvertPropToType<T>(buf, propertyType);
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }

            internal static ProcessorArchitecture GetProcessorArchitecture(IntPtr driverStoreHandle)
            {
                var propertyKey = DeviceHelper.DEVPKEY_DriverDatabase_ProcessorArchitecture;

                const int bufferSize = sizeof(short);
                var propertyBufferPtr = Marshal.AllocHGlobal(bufferSize);

                try
                {
                    NativeMethods.DriverStoreGetObjectProperty(
                        driverStoreHandle,
                        DriverStoreObjectType.DriverDatabase,
                        "SYSTEM",
                        ref propertyKey,
                        out var propertyType,
                        propertyBufferPtr,
                        bufferSize,
                        out var propertySize,
                        DriverStoreSetObjectPropertyFlags.None);

                    return (ProcessorArchitecture)Marshal.ReadInt16(propertyBufferPtr);
                }
                finally
                {
                    Marshal.FreeHGlobal(propertyBufferPtr);
                }
            }

            #region Enums

            [Flags]
            public enum DriverPackageEnumFilesFlags
            {
                None = 0,

                Copy = 0x00000001,              // Enumerate copy file operations

                Delete = 0x00000002,            // Enumerate delete file operations

                Rename = 0x00000004,            // Enumerate rename file operations

                Inf = 0x00000010,               // Enumerate driver package INF file

                Catalog = 0x00000020,           // Enumerate catalog file

                Binaries = 0x00000040,          // Enumerate binary files

                CopyInfs = 0x00000080,          // Enumerate copy INF files

                IncludeInfs = 0x00000100,       // Enumerate include INF files

                External = 0x00001000,          // Include external files in enumeration

                UniqueSource = 0x00002000,      // Only return files with unique sources

                UniqueDestination = 0x00004000  // Only return files with unique destinations
            }

            [Flags]
            public enum DriverPackageFlags
            {
                None = 0x00000000,

                Inbox = 0x00000001,

                Oem = 0x00000002,

                Published = 0x00000004,

                F6 = 0x00000008,

                BaseVersion = 0x00000010
            }

            [Flags]
            public enum DriverPackageGetPropertyFlags
            {
                None = 0x00000000,                  // Default settings
            }

            [Flags]
            public enum DriverPackageOpenFlags
            {
                None = 0,

                VersionOnly = 0x00000001,           // Open for version information only

                FilesOnly = 0x00000002,             // Open for file enumeration only

                DefaultLanguage = 0x00000004,       // Open with default language

                LocalizableStrings = 0x00000008,    // Open with localizable strings where applicable

                TargetOSVersion = 0x00000010,       // Open for use with target OS version

                StrictValidation = 0x00000020,      // Open with strict validation

                ClassSchemaOnly = 0x00000040,       // Open for class schema setting enumeration only

                LogTelemetry = 0x00000080,          // Open with telemetry logging

                PrimaryOnly = 0x00000100            // Open only the primary INF
            }

            [Flags]
            public enum DriverPackageVersionInfoFlags
            {
                None = 0x00000000,

                HAS_DEVICE_DRIVERS = 0x00000001,    // Driver package has device drivers

                PNP_LOCKDOWN = 0x00000002,   // Driver package has PnP lockdown enabled

                FORCE_BOOT_CRITICAL = 0x00000004,   // Force driver package to be boot critical

                FORCE_NOT_BOOT_CRITICAL = 0x00000008,   // Force driver package to NOT be boot critical

                HAS_DEVICE_CLASSES = 0x00000010,    // Driver package has device classes

                PRE_CONFIGURABLE = 0x00000020,    // Driver package has pre-configurable drivers

                HAS_DEVICES = 0x00000040,    // Driver package has devices

                HAS_INTERFACE_CLASSES = 0x00000080,    // Driver package has device interface classes

                HAS_PRIMITIVE_DRIVERS = 0x00000100,    // Driver package has primitive drivers

                HAS_DRIVERS = HAS_DEVICE_DRIVERS | HAS_PRIMITIVE_DRIVERS,

                HAS_CLASSES = HAS_DEVICE_CLASSES | HAS_INTERFACE_CLASSES,
            }

            public enum DRIVERSTORE_LOCK_LEVEL
            {
                NONE = 0,

                BASIC_PROTECTED,

                RUNTIME_ISOLATED,

                SYSTEM_PROTECTED,

                MAX,
            };

            // Driver Store Delete API
            [Flags]
            public enum DriverStoreDeleteFlags
            {
                INBOX = 0x00000001,    // Inbox driver package

                UNCONFIGURE = 0x00000002,    // Unconfigure driver package

                UNCONFIGURE_ONLY = 0x00000004,    // Unconfigure driver package only, without deleting it

                UNCONFIGURE_PRESERVE_STATE = 0x00010000,    // Preserve global state when unconfiguring, used with UNCONFIGURE flag only

                UNCONFIGURE_VALID = UNCONFIGURE_PRESERVE_STATE,

                VALID = INBOX
                    | UNCONFIGURE
                    | UNCONFIGURE_ONLY
                    | UNCONFIGURE_VALID
            }

            public enum DriverStoreObjectType
            {
                DriverDatabase = 0x00000001,

                DriverPackage = 0x00000002,

                DriverInfFile = 0x00000003,

                DriverFile,

                DeviceId,

                DeviceSetupClass,

                DeviceNode,

                DeviceInterfaceClass,

                DeviceInterface,

                DeviceContainer,

                DriverService,

                DriverRegKey,

                DevicePanel,
            }

            // Driver Store Update Devices API
            [Flags]
            public enum DriverStoreUpdateDevicesFlags
            {
                FORCE = 0x00000001,    // Force update of devices with applicable driver

                NULL_DRIVER = 0x00000002,    // Update devices with NULL driver

                ALTERNATIVE_DRIVER = 0x00000004,    // Update devices with next best alternative driver

                SOURCE_CONFIGURATIONS = 0x00010000,    // Source filter supplies configurations

                SOURCE_DEVICE_IDS = 0x00020000,    // Source filter supplies device IDs

                SOURCE_DRIVER_NAMES = 0x00040000,    // Source filter supplies driver node names

                TARGET_INSTANCE_IDS = 0x00100000,    // Target filter supplies device instance IDs

                TARGET_DEVICE_IDS = 0x00200000,    // Target filter supplies device IDs

                VALID = FORCE
                    | NULL_DRIVER
                    | ALTERNATIVE_DRIVER
                    | SOURCE_CONFIGURATIONS
                    | SOURCE_DEVICE_IDS
                    | SOURCE_DRIVER_NAMES
                    | TARGET_INSTANCE_IDS
                    | TARGET_DEVICE_IDS
            }

            /// <summary>
            ///     Processor Architecture (must match winnt.h)
            /// </summary>
            public enum ProcessorArchitecture : ushort
            {
                PROCESSOR_ARCHITECTURE_INTEL = 0,

                PROCESSOR_ARCHITECTURE_MIPS = 1,

                PROCESSOR_ARCHITECTURE_ALPHA = 2,

                PROCESSOR_ARCHITECTURE_PPC = 3,

                PROCESSOR_ARCHITECTURE_SHX = 4,

                PROCESSOR_ARCHITECTURE_ARM = 5,

                PROCESSOR_ARCHITECTURE_IA64 = 6,

                PROCESSOR_ARCHITECTURE_ALPHA64 = 7,

                PROCESSOR_ARCHITECTURE_MSIL = 8,

                PROCESSOR_ARCHITECTURE_AMD64 = 9,

                PROCESSOR_ARCHITECTURE_IA32_ON_WIN64 = 10,

                PROCESSOR_ARCHITECTURE_NEUTRAL = 11,

                PROCESSOR_ARCHITECTURE_ARM64 = 12,

                PROCESSOR_ARCHITECTURE_UNKNOWN = 0xFFFF,
            }

            internal enum DriverFileOperation : uint
            {
                Copy = 0,

                Delete,

                Rename
            }

            internal enum DriverFileType : uint
            {
                Inf = 0,

                Catalog,

                Binary,

                CopyInf,

                IncludeInf
            }

            [Flags]
            internal enum DriverStoreConfigureFlags : uint
            {
                None = 0x00000000,                   // Unknown

                Force = 0x00000001,    // Force configuration of non-configurable driver package

                ActiveOnly = 0x00000002,    // Configure already active configurations only

                SourceConfigurations = 0x00010000,    // Source filter supplies configurations

                SourceDeviceIds = 0x00020000,    // Source filter supplies device IDs

                TargetDeviceNodes = 0x00100000,    // Target filter supplies device instance IDs
            }

            [Flags]
            internal enum DriverStoreCopyFlags : uint
            {
                None = 0x00000000,                  // Default settings

                External = 0x00000001,              // Include externally included files when possible

                CopyInfs = 0x00000002,              // Include files referenced by copy INF directives

                SkipExistingCopyInfs = 0x00000004,  // Skip copy INFs that already exist in driver store

                SystemDefaultLocale = 0x00000008,   // Only copy files for system default locale

                Hardlink = 0x00000010,              // Hardlink files instead of copying them
            }

            [Flags]
            internal enum DriverStoreEnumFlags : uint
            {
                None = 0x00000000,                  // Default settings

                InboxOnly = 0x00000001,             // Enumerate only inbox driver packages

                OemOnly = 0x00000002,               // Enumerate only OEM driver packages

                PublishedOnly = 0x00000004,         // Enumerate only published driver packages

                Valid = InboxOnly | OemOnly | PublishedOnly,
            }

            /// <summary>
            ///     Flags for Importing the driver in the driver store.
            /// </summary>
            [Flags]
            internal enum DriverStoreImportFlags : uint
            {
                None = 0x00000000,                   // Unknown

                SkipTempCopy = 0x00000001,              // Skip temporary file copy step

                SkipExternalFileCheck = 0x00000002,     // Skip external file presence check

                NoRestorePoint = 0x00000004,            // Do not set a system restore point

                NonInteractive = 0x00000008,            // Enable non-interactive mode to not show any UI dialogs

                Replace = 0x00000020,                   // Replace existing driver package

                Hardlink = 0x00000040,                  // Hardlink files into Driver Store

                PublishSameName = 0x00000100,           // Publish same INF name instead of OEM INF name

                Inbox = 0x00000200,                     // Inbox driver package

                F6 = 0x00000400,                        // F6 driver package

                BaseVersion = 0x00000800,               // Base driver package version

                SystemDefaultLocale = 0x00001000,       // Only import files for system default locale

                SystemCritical = 0x00002000             // System critical driver package
            }

            /// <summary>
            ///     Flags for Importing the driver in the driver store.
            /// </summary>
            [Flags]
            internal enum DriverStoreOfflineAddDriverPackageFlags : uint
            {
                None = 0x00000000,                   // Unknown

                SkipInstall = 0x00000001,  //  Add the package to the driver store but skip the installation

                Inbox = 0x00000002,  // driver to be added is an inbox package

                F6 = 0x00000004, //  Add the package to the driver store as if the package was specified through the F6 mechanism

                SkipExternalFilePresenceCheck = 0x00000008, // Don't do presence check of external files in the driver package

                NoTempCopy = 0x00000010, // Do not perform the copy to the temporary directory

                UseHardLinks = 0x00000020, // Use hard links when importing to the driver store

                InstallOnly = 0x00000040, // Only install (reflect) a driver package that is already in the driver store.

                ReplacePackage = 0x00000080, // Replace the driver package if it is already present in the driver store.

                Force = 0x00000100, // Force offline reflection regardless of device class when importing to the driver store.

                BaseVersion = 0x00000200, // Driver package being added is the base version
            }

            /// <summary>
            ///     Flags for Opening the driver store.
            /// </summary>
            [Flags]
            internal enum DriverStoreOpenFlags : uint
            {
                None = 0x00000000,                   // Unknown

                Create = 0x00000001,                    // Create Driver Store if it doesnot exist

                Exclusive = 0x00000002,                 // Open Driver store for exclusive access
            }

            [Flags]
            internal enum DriverStorePublishFlags : uint
            {
                None = 0x00000000
            }

            [Flags]
            internal enum DriverStoreReflectCriticalFlags : uint
            {
                None = 0x00000000,

                Force = 0x00000001,    // Force reflection of non-boot critical driver packages

                Configurations = 0x00000002,    // Reflect driver configurations into driver database
            }

            [Flags]
            internal enum DriverStoreReflectFlags : uint
            {
                None = 0x00000000,

                FilesOnly = 0x00000001,    // Reflect driver files only

                ActiveDrivers = 0x00000002,    // Reflect previously reflected drivers for published name

                ExternalOnly = 0x00000004,    // Reflect external driver operations only

                Configurations = 0x00000008,    // Reflect driver configurations into driver database
            }

            [Flags]
            internal enum DriverStoreSetObjectPropertyFlags : uint
            {
                None = 0x00000000,
            }

            #endregion Enums

            #region Structs

            /// <summary>
            ///     The struct returned by DriverPackageGetVersionInfo.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct DriverPackageVersionInfo
            {
                public uint Size;

                public ProcessorArchitecture Architecture;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LOCALE_NAME_MAX_LENGTH)] public string LocaleName;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string ProviderName;

                public System.Runtime.InteropServices.ComTypes.FILETIME DriverDate;

                public ulong DriverVersion;

                public Guid ClassGuid;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string ClassName;

                public uint ClassVersion;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string CatalogFile;

                public DriverPackageVersionInfoFlags Flags;
            }

            /// <summary>
            ///     The DriverFile struct returned by DriverPackageEnumFilesW.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct DriverFile
            {
                internal DriverFileOperation Operation;

                internal string ExternalFile;

                internal DriverFileType Type;

                internal uint Flags;

                internal string SourceFile;

                internal string SourcePath;

                internal string DestinationFile;

                internal string DestinationPath;

                internal string ArchiveFile;

                internal string SecurityDescriptor;

                internal string SectionName;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct DriverPackageInfo
            {
                public ProcessorArchitecture ProcessorArchitecture;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LOCALE_NAME_MAX_LENGTH)] public string LocaleName;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public string PublishedInfName;

                public DriverPackageFlags Flags;
            };

            #endregion Structs
        }
    }
}