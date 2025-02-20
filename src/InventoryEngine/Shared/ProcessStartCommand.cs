using System;
using System.Diagnostics;
using InventoryEngine.Tools;

namespace InventoryEngine.Shared
{
    internal class ProcessStartCommand
    {
        internal string Arguments { get; set; }

        internal string FileName { get; set; }

        internal ProcessStartCommand(string filename)
                            : this(filename, string.Empty)
        {
        }

        /// <summary>
        ///     Cleans up process start command and parameters
        /// </summary>
        /// <param name="filename"> executable path </param>
        /// <param name="args"> arguments to the executable </param>
        /// <exception cref="ArgumentNullException">
        ///     The values of 'filename' and 'args' cannot be null.
        /// </exception>
        internal ProcessStartCommand(string filename, string args)
        {
            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            FileName = filename.Trim().Trim('"', '\'');
            Arguments = args.Trim();
        }

        internal static bool TryParse(string command, out ProcessStartCommand result)
        {
            try
            {
                result = ProcessTools.SeparateArgsFromCommand(command);
            }
            catch (Exception ex)
            {
                result = null;
                Debug.WriteLine(ex);
            }

            return result != null;
        }

        internal ProcessStartInfo ToProcessStartInfo() => new ProcessStartInfo(FileName, Arguments) { UseShellExecute = true };

        internal string ToCommandLine() => ToString();

        public override string ToString() => string.IsNullOrEmpty(Arguments) ? $"\"{FileName}\"" : $"\"{FileName}\" {Arguments}";
    }
}