using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using InventoryEngine.Extensions;
using InventoryEngine.Shared;

namespace InventoryEngine.Tools
{
    internal static class ProcessTools
    {
        internal static bool Is64BitProcess => IntPtr.Size == 8;

        private static readonly char[] SeparateArgsFromCommandInvalidChars =
                    Path.GetInvalidFileNameChars().Concat(new[] { ',', ';' }).ToArray();

        /// <summary>
        ///     Attempts to separate filename (or filename with path) from the supplied arguments.
        /// </summary>
        /// <param name="fullCommand"> </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentNullException">
        ///     The value of 'fullCommand' cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException"> fullCommand can't be empty </exception>
        /// <exception cref="FormatException"> Filename is in invalid format </exception>
        internal static ProcessStartCommand SeparateArgsFromCommand(string fullCommand)
        {
            if (fullCommand == null)
            {
                throw new ArgumentNullException(nameof(fullCommand));
            }

            // Get rid of whitespaces
            fullCommand = fullCommand.Trim();

            if (string.IsNullOrEmpty(fullCommand))
            {
                throw new ArgumentException("Error_SeparateArgsFromCommand_Empty", nameof(fullCommand));
            }

            var firstDot = fullCommand.IndexOf('.');
            if (firstDot < 0)
            {
                return SeparateNonDottedCommand(fullCommand);
            }

            // Check if the path is in format: ExecutableName C:\Argname.exe
            var pathRoot = fullCommand.IndexOf(":\\", StringComparison.InvariantCulture);
            var firstSpace = fullCommand.IndexOf(' ');
            if (firstSpace >= 0 && firstSpace < pathRoot)
            {
                var filenameBreaker = fullCommand.IndexOfAny(SeparateArgsFromCommandInvalidChars, 0, pathRoot - 1);
                if (filenameBreaker < 0)
                {
                    var slashIndex = fullCommand.IndexOf('\\');
                    if (slashIndex >= 0 && slashIndex > pathRoot)
                    {
                        var rootSpace = fullCommand.LastIndexOf(' ', pathRoot);
                        return new ProcessStartCommand(fullCommand.Substring(0, rootSpace).TrimEnd(),
                            fullCommand.Substring(rootSpace));
                    }
                }
            }

            // Check if the path is contained inside of quotation marks. Assume that the quotation
            // mark must come before the dot. Otherwise, it is likely that the arguments use quotations.
            var pathEnd = fullCommand.IndexOf('"', 0, firstDot);
            if (pathEnd >= 0)
            {
                // If yes, find the closing quotation mark and set its index as path end
                pathEnd = fullCommand.IndexOf('"', pathEnd + 1);

                if (pathEnd < 0)
                {
                    // If no ending quote has been found, explode gracefully.
                    throw new FormatException("Error_SeparateArgsFromCommand_MissingQuotationMark");
                }
                pathEnd++; //?
            }

            // If quotation marks were missing, check for any invalid characters after last dot in
            // case of eg: c:\test.dir thing\filename.exe?0 used to get icons
            if (pathEnd >= 0)
            {
                return SeparateCommand(fullCommand, pathEnd);
            }

            {
                var endIndex = 0;
                while (true)
                {
                    var dot = fullCommand.IndexOf('.', endIndex);
                    if (dot < 0)
                    {
                        break;
                    }

                    var filenameBreaker = fullCommand.IndexOfAny(SeparateArgsFromCommandInvalidChars, dot);
                    var space = fullCommand.IndexOf(' ', dot);
                    if (filenameBreaker < 0)
                    {
                        if (space < 0)
                        {
                            break;
                        }

                        filenameBreaker = space;
                    }

                    var dash = fullCommand.IndexOf('\\', dot);
                    if (filenameBreaker < dash || dash < 0)
                    {
                        pathEnd = space < 0 ? filenameBreaker : space;
                        break;
                    }

                    var nextBreaker = fullCommand.IndexOfAny(SeparateArgsFromCommandInvalidChars, filenameBreaker + 1);
                    var nextDash = fullCommand.IndexOf('\\', filenameBreaker + 1);

                    if (nextBreaker > 0 && (nextDash < 0 || nextBreaker < nextDash))
                    {
                        var nextDot = fullCommand.IndexOf('.', filenameBreaker + 1);
                        if (nextDot < 0 || nextBreaker < nextDot)
                        {
                            pathEnd = space < 0 ? filenameBreaker : space;
                            break;
                        }
                    }

                    endIndex = dash;
                }
            }

            return SeparateCommand(fullCommand, pathEnd);
        }

        private static ProcessStartCommand SeparateCommand(string fullCommand, int splitIndex)
        {
            // Begin extracting filename and arguments
            string filename;
            var args = string.Empty;

            if (splitIndex < 0 || splitIndex >= fullCommand.Length)
            {
                // Looks like there were no arguments, assume whole command is a filename
                filename = fullCommand;
            }
            else
            {
                // pathEnd shows the end of the filename (and start of the arguments)
                filename = fullCommand.Substring(0, splitIndex).TrimEnd();
                args = fullCommand.Substring(splitIndex).TrimStart();
            }

            filename = filename.Trim('"'); // Get rid of the quotation marks
            return new ProcessStartCommand(filename, args);
        }

        private static ProcessStartCommand SeparateNonDottedCommand(string fullCommand)
        {
            // Look for the first root of a path
            var pathRoot = fullCommand.IndexOf(":\\", StringComparison.InvariantCulture);
            var pathRootEnd = pathRoot < 0 ? 0 : pathRoot + 2;

            var breakChars = SeparateArgsFromCommandInvalidChars.Except(new[] { '\\' }).ToArray();

            // Check if there are any invalid path chars before the start we found. If yes, our path
            // is most likely an argument.
            if (pathRootEnd > 0 && fullCommand.IndexOfAny(breakChars, 0, pathRootEnd - 2) >= 0)
            {
                pathRootEnd = 0;
            }

            var breakIndex = fullCommand.IndexOfAny(breakChars, pathRootEnd);

            // If there are no invalid path chars, it's probably just a naked filename or directory path.
            if (breakIndex < 0)
            {
                return new ProcessStartCommand(fullCommand.Trim('"'));
            }

            // The invalid char has to have at least 1 space before it to count as an argument.
            // Otherwise the input is likely garbage.
            if (breakIndex > 0 && fullCommand[breakIndex - 1] == ' ')
            {
                return new ProcessStartCommand(fullCommand.Substring(0, breakIndex - 1).TrimEnd(),
                    fullCommand.Substring(breakIndex));
            }

            throw new FormatException("Error_SeparateArgsFromCommand_NoDot\n" + fullCommand);
        }
    }
}