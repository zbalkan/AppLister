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
            public enum DriverPackageFlags
            {
                None = 0x00000000,

                Inbox = 0x00000001,

                Oem = 0x00000002,

                Published = 0x00000004,

                F6 = 0x00000008,

                BaseVersion = 0x00000010
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
            internal enum DriverStoreSetObjectPropertyFlags : uint
            {
                None = 0x00000000,
            }

            #endregion Enums

            #region Structs

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