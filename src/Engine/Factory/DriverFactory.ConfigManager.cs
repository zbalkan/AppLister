using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Engine.Factory
{
    internal partial class DriverFactory
    {
        public static partial class ConfigManager
        {
            public static List<DriverStoreEntry> FillDeviceInfo(List<DriverStoreEntry> driverStoreEntries)
            {
                var devicesInfo = GetDeviceDriverInfo();

                foreach (var driverStoreEntry in driverStoreEntries)
                {
                    var deviceInfo = devicesInfo.OrderByDescending(d => d.IsPresent)?
                        .FirstOrDefault(e =>
                        string.Equals(e.DriverInf, driverStoreEntry.DriverPublishedName, StringComparison.OrdinalIgnoreCase)
                        && e.DriverVersion == driverStoreEntry.DriverVersion
                        && e.DriverDate == driverStoreEntry.DriverDate);
                    if (deviceInfo != null)
                    {
                        driverStoreEntry.DeviceId = deviceInfo?.DeviceId;
                        driverStoreEntry.DeviceName = deviceInfo?.DeviceName;
                        driverStoreEntry.DriverArchitecture = deviceInfo?.DriverArchitecture ?? NativeDriverStore.ProcessorArchitecture.PROCESSOR_ARCHITECTURE_UNKNOWN;
                    }
                }

                return driverStoreEntries;
            }

            internal static T GetClassProperty<T>(Guid classGuid, DevPropKey propertyKey)
            {
                // First pass: request required buffer size
                uint propertySize = 0;
                var cr = NativeMethods.CM_Get_Class_Property(
                    classGuid,
                    ref propertyKey,
                    out _,
                    IntPtr.Zero,
                    ref propertySize,
                    0);

                // If the call succeeded but propertySize == 0, the property exists but is empty
                if (cr == ConfigManagerResult.Success && propertySize == 0)
                {
                    return default;
                }

                // If the property is missing or an unexpected error occurred, return default
                if (cr == ConfigManagerResult.NoSuchValue ||
                    (cr != ConfigManagerResult.BufferSmall && cr != ConfigManagerResult.Success))
                {
                    return default;
                }

                // At this point, cr == BufferSmall ⇒ propertySize holds the exact size needed
                if (propertySize > int.MaxValue)
                {
                    throw new OverflowException($"Property size {propertySize} exceeds maximum buffer length.");
                }
                var bufferSize = (int)propertySize;

                // Second pass: allocate exactly the right amount of memory and fetch
                var buf = Marshal.AllocHGlobal(bufferSize);
                try
                {
                    cr = NativeMethods.CM_Get_Class_Property(
                        classGuid,
                        ref propertyKey,
                        out var propertyType,
                        buf,
                        ref propertySize,
                        0);

                    if (cr != ConfigManagerResult.Success)
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

            private static List<DeviceDriverInfo> GetDeviceDriverInfo()
            {
                var deviceDriverInfos = new List<DeviceDriverInfo>();

                var deviceListLength = 0;
                if (NativeMethods.CM_Get_Device_ID_List_Size(
                    ref deviceListLength,
                    null,
                    0) == ConfigManagerResult.Success)
                {
                    var buffer = new byte[(deviceListLength * sizeof(char)) + 2];
                    if (NativeMethods.CM_Get_Device_ID_List(
                        null,
                        buffer,
                        deviceListLength,
                        CM_GETIDLIST_FILTER.NONE) == ConfigManagerResult.Success)
                    {
                        var deviceIds = Encoding.Unicode.GetString(buffer).Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var deviceId in deviceIds)
                        {
                            uint devInst = 0;
                            if (NativeMethods.CM_Locate_DevNode(
                                ref devInst,
                                deviceId,
                                CM_LOCATE_DEVNODE_FLAG.CM_LOCATE_DEVNODE_PHANTOM) == ConfigManagerResult.Success)
                            {
                                try
                                {
                                    var di = new DeviceDriverInfo(
                                        GetDevNodeProperty<string>(devInst, DeviceHelper.DEVPKEY_Device_InstanceId),
                                        GetDevNodeProperty<string>(devInst, DeviceHelper.DEVPKEY_Device_FriendlyName)
                                            ?? GetDevNodeProperty<string>(devInst, DeviceHelper.DEVPKEY_Device_DeviceDesc),
                                        GetDevNodeProperty<string>(devInst, DeviceHelper.DEVPKEY_Device_DriverInfPath),
                                        GetDevNodeProperty<DateTime>(devInst, DeviceHelper.DEVPKEY_Device_DriverDate),
                                        GetDevNodeProperty<Version>(devInst, DeviceHelper.DEVPKEY_Device_DriverVersion),
                                        IsDevicePresent(devInst),
                                        GetDevNodeProperty<NativeDriverStore.ProcessorArchitecture>(devInst, DeviceHelper.DEVPKEY_DriverPackage_ProcessorArchitecture));
                                    deviceDriverInfos.Add(di);
                                }
                                catch (Win32Exception)
                                {
                                    Debug.WriteLine($"Failed to retrieve properties for device ID: {deviceId} (DevInst: {devInst})");
                                }
                            }
                        }
                    }
                }

                return deviceDriverInfos;
            }

            private static T GetDevNodeProperty<T>(uint devInst, DevPropKey key)
            {
                uint size = 0;
                var cr = NativeMethods.CM_Get_DevNode_Property(devInst, ref key,
                                                               out _,
                                                               IntPtr.Zero, ref size, 0);

                if (cr == ConfigManagerResult.NoSuchValue)   // truly absent
                {
                    return default;
                }

                if (cr != ConfigManagerResult.BufferSmall)    // unexpected error
                {
                    throw new Win32Exception((int)cr);
                }

                if (size > int.MaxValue)
                {
                    throw new OverflowException($"Property size {size} exceeds maximum buffer length.");
                }
                var buf = Marshal.AllocHGlobal((int)size);
                try
                {
                    cr = NativeMethods.CM_Get_DevNode_Property(devInst, ref key,
                                                               out var type,
                                                               buf, ref size, 0);
                    if (cr != ConfigManagerResult.Success)
                    {
                        throw new Win32Exception((int)cr);
                    }

                    return DeviceHelper.ConvertPropToType<T>(buf, type);
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            }

            private static bool? IsDevicePresent(uint devInst)
            {
                var result = NativeMethods.CM_Get_DevNode_Status(out _, out _, devInst, 0);

                if (result == ConfigManagerResult.Success)
                {
                    return true;
                }
                else if (result == ConfigManagerResult.NoSuchDevnode)
                {
                    return false;
                }
                else
                {
                    return null;
                }
            }

            #region Enums
            public enum ConfigManagerResult : uint
            {
                Success = 0x00000000,
                Default = 0x00000001,
                OutOfMemory = 0x00000002,
                InvalidPointer = 0x00000003,
                InvalidFlag = 0x00000004,
                InvalidDevnode = 0x00000005,
                InvalidDevinst = InvalidDevnode,
                InvalidResDes = 0x00000006,
                InvalidLogConf = 0x00000007,
                InvalidArbitrator = 0x00000008,
                InvalidNodelist = 0x00000009,
                DevnodeHasReqs = 0x0000000A,
                DevinstHasReqs = DevnodeHasReqs,
                InvalidResourceid = 0x0000000B,
                NoSuchDevnode = 0x0000000D,
                NoSuchDevinst = NoSuchDevnode,
                NoMoreLogConf = 0x0000000E,
                NoMoreResDes = 0x0000000F,
                AlreadySuchDevnode = 0x00000010,
                AlreadySuchDevinst = AlreadySuchDevnode,
                InvalidRangeList = 0x00000011,
                InvalidRange = 0x00000012,
                Failure = 0x00000013,
                NoSuchLogicalDev = 0x00000014,
                CreateBlocked = 0x00000015,
                RemoveVetoed = 0x00000017,
                ApmVetoed = 0x00000018,
                InvalidLoadType = 0x00000019,
                BufferSmall = 0x0000001A,
                NoArbitrator = 0x0000001B,
                NoRegistryHandle = 0x0000001C,
                RegistryError = 0x0000001D,
                InvalidDeviceId = 0x0000001E,
                InvalidData = 0x0000001F,
                InvalidApi = 0x00000020,
                DevloaderNotReady = 0x00000021,
                NeedRestart = 0x00000022,
                NoMoreHwProfiles = 0x00000023,
                DeviceNotThere = 0x00000024,
                NoSuchValue = 0x00000025,
                WrongType = 0x00000026,
                InvalidPriority = 0x00000027,
                NotDisableable = 0x00000028,
                FreeResources = 0x00000029,
                QueryVetoed = 0x0000002A,
                CantShareIrq = 0x0000002B,
                NoDependent = 0x0000002C,
                SameResources = 0x0000002D,
                NoSuchRegistryKey = 0x0000002E,
                InvalidMachinename = 0x0000002F,   // NT ONLY
                RemoteCommFailure = 0x00000030,   // NT ONLY
                MachineUnavailable = 0x00000031,   // NT ONLY
                NoCmServices = 0x00000032,   // NT ONLY
                AccessDenied = 0x00000033,   // NT ONLY
                CallNotImplemented = 0x00000034,
                InvalidProperty = 0x00000035,
                DeviceInterfaceActive = 0x00000036,
                NoSuchDeviceInterface = 0x00000037,
                InvalidReferenceString = 0x00000038,
                InvalidConflictList = 0x00000039,
                InvalidIndex = 0x0000003A,
                InvalidStructureSize = 0x0000003B
            }

            //
            // Flags for CM_Get_Device_ID_List, CM_Get_Device_ID_List_Size
            //
            [Flags]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1157:Composite enum value contains undefined flag", Justification = "<Pending>")]
            private enum CM_GETIDLIST_FILTER : uint
            {
                NONE = 0x00000000,
                ENUMERATOR = 0x00000001,
                SERVICE = 0x00000002,
                EJECTRELATIONS = 0x00000004,
                REMOVALRELATIONS = 0x00000008,
                POWERRELATIONS = 0x00000010,
                BUSRELATIONS = 0x00000020,
                TRANSPORTRELATIONS = 0x00000080,
                PRESENT = 0x00000100,
                CLASS = 0x00000200,
                DONOTGENERATE = 0x10000040,
                BITS = DONOTGENERATE | ENUMERATOR | SERVICE | EJECTRELATIONS | REMOVALRELATIONS | POWERRELATIONS | BUSRELATIONS | TRANSPORTRELATIONS | PRESENT | CLASS,
            }

            //
            // Flags for CM_Locate_DevNode
            //
            private enum CM_LOCATE_DEVNODE_FLAG : uint
            {
                CM_LOCATE_DEVNODE_NORMAL = 0x00000000,
                CM_LOCATE_DEVNODE_PHANTOM = 0x00000001,
                CM_LOCATE_DEVNODE_CANCELREMOVE = 0x00000002,
                CM_LOCATE_DEVNODE_NOVALIDATION = 0x00000004,
                CM_LOCATE_DEVNODE_BITS = 0x00000007,
            }

            #endregion Enums
        }
    }
}
