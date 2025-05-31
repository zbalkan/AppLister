using System;
using System.Runtime.InteropServices;

namespace Engine.Factory
{
    internal partial class DriverFactory
    {
        public static partial class ConfigManager
        {
            /// <summary>
            /// The managed interop layer to CfgMgr32.dll
            /// </summary>
            private static class NativeMethods
            {
                [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern ConfigManagerResult CM_Get_Class_Property(
                    Guid classGUID,
                    ref DevPropKey propertyKey,
                    out DevPropType propertyType,
                    IntPtr buffer,
                    ref uint bufferSize,
                    uint flags);

                [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern ConfigManagerResult CM_Get_Device_ID_List(string filter, byte[] buffer, int bufferLength, CM_GETIDLIST_FILTER flags);

                [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern ConfigManagerResult CM_Get_Device_ID_List_Size(ref int length, string filter, CM_GETIDLIST_FILTER flags);

                [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern ConfigManagerResult CM_Get_DevNode_Property(
                    uint devInst,
                    ref DevPropKey propertyKey,
                    out DevPropType propertyType,
                    IntPtr buffer,
                    ref uint bufferSize,
                    uint flags);

                [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern ConfigManagerResult CM_Get_DevNode_Status(
                  out uint status,
                  out uint problemNumber,
                  uint devInst,
                  uint ulFlags);

                [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern ConfigManagerResult CM_Locate_DevNode(ref uint devInst, string deviceID, CM_LOCATE_DEVNODE_FLAG flags);
            }
        }
    }
}
