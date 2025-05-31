using System;
using System.Runtime.InteropServices;

namespace Engine.Factory
{
    internal partial class DriverFactory
    {
        public partial class NativeDriverStore
        {
            /// <summary>
            ///     The managed interop layer to drvstore.dll
            /// </summary>
            internal static class NativeMethods
            {
                public delegate bool EnumDriverPackageDelegate(
                                IntPtr driverStoreHandle,
                                [MarshalAs(UnmanagedType.LPWStr, SizeConst = 256)]
                string driverStoreFilename,
                                DriverPackageInfo driverPackageInfo,
                                IntPtr lParam);

                /// <summary>
                ///     Close the driver store.
                /// </summary>
                /// <param name="driverStoreHandle">
                ///     handle to the driver store.
                /// </param>
                /// <returns>
                ///     True on success. False on failure.
                /// </returns>
                [DllImport("drvstore.dll", SetLastError = true)]
                internal static extern bool DriverStoreClose(
                     IntPtr driverStoreHandle);

                [DllImport("drvstore.dll", EntryPoint = "DriverStoreEnumW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern bool DriverStoreEnum(
                                IntPtr driverStoreHandle,
                                DriverStoreEnumFlags flags,
                                EnumDriverPackageDelegate CallbackRoutine,
                                IntPtr lParam);

                [DllImport("drvstore.dll", EntryPoint = "DriverStoreGetObjectPropertyW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern bool DriverStoreGetObjectProperty(
                                IntPtr driverStoreHandle,
                                DriverStoreObjectType objectType,
                                string objectName,
                                ref DevPropKey propertyKey,
                                out DevPropType propertyType,
                                IntPtr propertyBuffer,
                                int bufferSize,
                                out uint propertySize,
                                DriverStoreSetObjectPropertyFlags flag);

                /// <summary>
                ///     Open the driver store.
                /// </summary>
                /// <param name="targetSystemPath">
                ///     The path to the "windows" directory on the image.
                /// </param>
                /// <param name="targetBootDrive">
                ///     The path to the boot drive on the image.
                /// </param>
                /// <param name="flags">
                ///     Flags to Open the driver store.
                /// </param>
                /// <param name="transactionHandle">
                ///     transaction handle
                /// </param>
                /// <returns>
                ///     Handle to the driver store on success. IntPtr.Zero on failure.
                /// </returns>
                [DllImport("drvstore.dll", EntryPoint = "DriverStoreOpenW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern IntPtr DriverStoreOpen(
                    string targetSystemPath,
                    string targetBootDrive,
                    DriverStoreOpenFlags flags,
                    IntPtr transactionHandle);
            }
        }
    }
}