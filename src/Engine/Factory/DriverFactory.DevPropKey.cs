using System;
using System.Runtime.InteropServices;

namespace Engine.Factory
{
    internal partial class DriverFactory
    {
        /// <summary>
        ///     The DEVPROPKEY structure represents a device property key for a device property in
        ///     the unified device property model.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct DevPropKey
        {
            public Guid fmtid;

            public uint pid;

            public DevPropKey(Guid fmtid, uint pid)
            {
                this.fmtid = fmtid;
                this.pid = pid;
            }

            public DevPropKey(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k, uint pid)
            {
                fmtid = new Guid(a, b, c, d, e, f, g, h, i, j, k);
                this.pid = pid;
            }
        }
    }
}