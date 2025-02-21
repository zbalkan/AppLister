using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace InventoryEngine.Extensions
{
    /// <summary>
    ///     A utility class to determine a process parent. By Simon Mourier https://stackoverflow.com/a/3346055/4309247
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ParentProcessUtilities
    {
        // These members must match PROCESS_BASIC_INFORMATION
        internal IntPtr Reserved1;

        internal IntPtr PebBaseAddress;

        internal IntPtr Reserved2_0;

        internal IntPtr Reserved2_1;

        internal IntPtr UniqueProcessId;

        internal IntPtr InheritedFromUniqueProcessId;

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

        /// <summary>
        ///     Gets the parent process of a specified process.
        /// </summary>
        /// <param name="handle">
        ///     The process handle.
        /// </param>
        /// <returns>
        ///     An instance of the Process class.
        /// </returns>
        /// <exception cref="Win32Exception">
        ///     The status may not be obtained properly due to permissions.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Exceptions", "TI8110:Do not silently ignore exceptions", Justification = "Ignore invalid pointer as the parent may have been killed.")]
        internal static Process GetParentProcess(IntPtr handle)
        {
            var pbi = new ParentProcessUtilities();
            var status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out _);
            if (status != 0)
            {
                throw new Win32Exception(status);
            }

            try
            {
                return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
            }
            catch (ArgumentException)
            {
                // not found
                return null;
            }
        }
    }
}