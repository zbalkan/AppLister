using System;
using System.IO;

namespace InventoryEngine.Tools
{
    internal static class FilesystemTools
    {
        /// <summary>
        /// Check the architecture of the executable. E.g. 64bit.
        /// Returns Unknown if the architecture is unsupported or not specified.
        /// </summary>
        /// <param name="filename">Full path to the executable file.</param>
        public static MachineType CheckExecutableMachineType(string filename)
        {
            if (!filename.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new IOException("Not a Windows .exe file.");
            }

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                stream.Position = 0x3c;
                var fileData = new byte[1024];

                var bytesRead = stream.Read(fileData, 0, 1024);

                for (var i = 0; i < bytesRead; i++)
                {
                    // Look for the PE signature (PE\0\0)
                    if (i + 5 >= bytesRead)
                    {
                        break;
                    }

                    if (fileData[i] != 0x50)
                    {
                        continue;
                    }

                    if (fileData[i + 1] != 0x45 || fileData[i + 2] != 0 || fileData[i + 3] != 0)
                    {
                        continue;
                    }

                    // Join two bytes representing the architecture
                    var machineId = fileData[i + 5] << 8 | fileData[i + 4];
                    switch (machineId)
                    {
                        case 0x8664:
                            return MachineType.X64;
                        case 0x14c:
                            return MachineType.X86;
                        case 0x200:
                            return MachineType.Ia64;
                        default:
                            return MachineType.Unknown;
                    }
                }
            }

            return MachineType.Unknown;
        }
    }
}