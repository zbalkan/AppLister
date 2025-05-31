namespace Engine.Factory
{
    /// <summary>
    ///     The DEVPROPTYPE data type represents the property-data-type identifier that specifies
    ///     the data type of a device property value in the unified device property model.
    /// </summary>
    internal enum DevPropType : uint
    {
        TYPEMOD_ARRAY = 0x00001000,  // array of fixed-sized data elements

        TYPEMOD_LIST = 0x00002000,  // list of variable-sized data elements

        // Property data types.
        Empty = 0x00000000, // nothing, no property data

        Null = 0x00000001, // null property data

        Sbyte = 0x00000002, // 8-bit signed int (sbyte)

        Byte = 0x00000003, // 8-bit unsigned int (byte)

        Int16 = 0x00000004, // 16-bit signed int (short)

        Uint16 = 0x00000005, // 16-bit unsigned int (ushort)

        Int32 = 0x00000006, // 32-bit signed int (long)

        Uint32 = 0x00000007, // 32-bit unsigned int (ulong)

        Int64 = 0x00000008, // 64-bit signed int (long64)

        Uint64 = 0x00000009, // 64-bit unsigned int (ulong64)

        Float = 0x0000000a, // 32-bit floating-point (float)

        Double = 0x0000000b, // 64-bit floating-point (double)

        Decimal = 0x0000000c, // 128-bit data (decimal)

        Guid = 0x0000000d, // 128-bit unique identifier (guid)

        Currency = 0x0000000e, // 64 bit signed int currency value (currency)

        Date = 0x0000000f, // date (date)

        FileTime = 0x00000010, // file time (filetime)

        Boolean = 0x00000011, // 8-bit boolean (devprop_boolean)

        String = 0x00000012, // null-terminated string

        StringList = (String | TYPEMOD_LIST), // multi-sz string list

        SecurityDescriptor = 0x00000013, // self-relative binary security_descriptor

        SecurityDescriptorString = 0x00000014, // security descriptor string (sddl format)

        Devpropkey = 0x00000015, // device property key (devpropkey)

        Devproptype = 0x00000016, // device property type (devproptype)

        Binary = (Byte | TYPEMOD_ARRAY), // custom binary data

        Error = 0x00000017, // 32-bit win32 system error code

        Ntstatus = 0x00000018, // 32-bit ntstatus code

        StringIndirect = 0x00000019, // string resource (@[path\]<dllname>,-<strid>)
    }
}