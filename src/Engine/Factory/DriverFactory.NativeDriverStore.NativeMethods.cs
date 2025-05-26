using System;
using System.Runtime.InteropServices;
using System.Text;

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

                public delegate bool EnumFilesDelegate(IntPtr driverPackageHandle, IntPtr pDriverFile, IntPtr lParam);

                public delegate bool EnumObjectsDelegate(
                                IntPtr hDriverStore,
                                DriverStoreObjectType ObjectType,
                                [MarshalAs(UnmanagedType.LPWStr, SizeConst = MAX_PATH)] string objectName,
                                IntPtr lParam);

                [DllImport("drvstore.dll", EntryPoint = "DriverPackageClose", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern void DriverPackageClose(
                                IntPtr driverPackageHandle);

                [DllImport("drvstore.dll", EntryPoint = "DriverPackageEnumFilesW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern bool DriverPackageEnumFilesW(
                                IntPtr driverPackageHandle,
                                IntPtr enumContext,
                                DriverPackageEnumFilesFlags flags,
                                EnumFilesDelegate callbackRoutine,
                                IntPtr lParam);

                [DllImport("drvstore.dll", EntryPoint = "DriverPackageGetPropertyW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern bool DriverPackageGetProperty(
                                IntPtr driverPackageHandle,
                                IntPtr enumContext,
                                string sectionName,
                                IntPtr propertyKey,
                                IntPtr propertyType,
                                IntPtr propertyBuffer,
                                uint bufferSize,
                                IntPtr propertySize,
                                DriverPackageGetPropertyFlags flags);

                [DllImport("drvstore.dll", EntryPoint = "DriverPackageGetVersionInfoW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern bool DriverPackageGetVersionInfo(
                                IntPtr driverPackageHandle,
                                IntPtr pVersionInfo);

                [DllImport("drvstore.dll", EntryPoint = "DriverPackageOpenW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern IntPtr DriverPackageOpen(
                                string driverPackageFilename,
                                ProcessorArchitecture processorArchitecture,
                                string localeName,
                                DriverPackageOpenFlags flags,
                                IntPtr resolveContext);

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

                [DllImport("drvstore.dll", EntryPoint = "DriverStoreConfigureW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern uint DriverStoreConfigure(
                                IntPtr hDriverStore,
                                string DriverStoreFilename,
                                DriverStoreConfigureFlags Flags,
                                string SourceFilter,
                                string TargetFilter);

                [DllImport("drvstore.dll", EntryPoint = "DriverStoreCopyW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern uint DriverStoreCopy(
                                IntPtr driverPackageHandle,
                                string driverPackageFilename,
                                ProcessorArchitecture processorArchitecture,
                                IntPtr localeName,
                                DriverStoreCopyFlags flags,
                                string destinationPath);

                [DllImport("drvstore.dll", EntryPoint = "DriverStoreDeleteW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern uint DriverStoreDelete(
                                IntPtr hDriverStore,
                                string driverStoreFilename,
                                DriverStoreDeleteFlags flags);

                [DllImport("drvstore.dll", EntryPoint = "DriverStoreEnumW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern bool DriverStoreEnum(
                                IntPtr driverStoreHandle,
                                DriverStoreEnumFlags flags,
                                EnumDriverPackageDelegate CallbackRoutine,
                                IntPtr lParam);

                [DllImport("drvstore.dll", EntryPoint = "DriverStoreEnumObjectsW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern bool DriverStoreEnumObjects(
                                IntPtr hDriverStore,
                                DriverStoreObjectType objectType,
                                DRIVERSTORE_LOCK_LEVEL flags,
                                EnumObjectsDelegate callbackRoutine,
                                IntPtr lParam
                            );

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
                ///     Import the driver package to the driver store.
                /// </summary>
                /// <param name="driverStoreHandle">
                ///     handle to the driver store.
                /// </param>
                /// <param name="driverPackageFileName">
                ///     the name of the driver package file.
                /// </param>
                /// <param name="processorArchitecture">
                ///     the processor architecture.
                /// </param>
                /// <param name="localeName">
                ///     the loacle for the package.
                /// </param>
                /// <param name="flags">
                ///     the flags for import.
                /// </param>
                /// <param name="driverStoreFileName">
                ///     the driver store file name buffer.
                /// </param>
                /// <param name="driverStoreFileNameSize">
                ///     the driver store file name size.
                /// </param>
                /// <returns>
                ///     Result code for operation.
                /// </returns>
                [DllImport("drvstore.dll", EntryPoint = "DriverStoreImportW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern uint DriverStoreImport(
                    IntPtr driverStoreHandle,
                    string driverPackageFileName,
                    ProcessorArchitecture processorArchitecture,
                    string localeName,
                    DriverStoreImportFlags flags,
                    StringBuilder driverStoreFileName,
                    int driverStoreFileNameSize);

                [DllImport("drvstore.dll", EntryPoint = "DriverStoreOfflineAddDriverPackageW", SetLastError = true, CharSet = CharSet.Unicode)]
                internal static extern uint DriverStoreOfflineAddDriverPackage(
                                string DriverPackageInfPath,
                                DriverStoreOfflineAddDriverPackageFlags Flags,
                                IntPtr Reserved,
                                ushort ProcessorArchitecture,
                                string LocaleName,
                                StringBuilder DestInfPath,
                                ref int cchDestInfPath,
                                string TargetSystemRoot,
                                string TargetSystemDrive);

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