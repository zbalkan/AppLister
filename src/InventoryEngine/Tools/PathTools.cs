using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InventoryEngine.Extensions;
using Microsoft.Win32;

namespace InventoryEngine.Tools
{
    internal static class PathTools
    {
        private static readonly char[] PathTrimChars = {
            '\\',
            '/',
            '"',

            // SPACE
            '\u0020',

            // NO-BREAK SPACE
            '\u00A0',

            // OGHAM SPACE MARK
            '\u1680',

            // EN QUAD
            '\u2000',

            // EM QUAD
            '\u2001',

            // EN SPACE
            '\u2002',

            // EM SPACE
            '\u2003',

            // THREE-PER-EM SPACE
            '\u2004',

            // FOUR-PER-EM SPACE
            '\u2005',

            // SIX-PER-EM SPACE
            '\u2006',

            // FIGURE SPACE
            '\u2007',

            // PUNCTUATION SPACE
            '\u2008',

            // THIN SPACE
            '\u2009',

            // HAIR SPACE
            '\u200A',

            // NARROW NO-BREAK SPACE
            '\u202F',

            // MEDIUM MATHEMATICAL SPACE
            '\u205F',

            // and IDEOGRAPHIC SPACE
            '\u3000',

            // LINE SEPARATOR
            '\u2028',

            // PARAGRAPH SEPARATOR
            '\u2029',

            // CHARACTER TABULATION
            '\u0009',

            // LINE FEED
            '\u000A',

            // LINE TABULATION
            '\u000B',

            // FORM FEED
            '\u000C',

            // CARRIAGE RETURN
            '\u000D',

            // NEXT LINE
            '\u0085'
        };

        /// <summary>
        ///     Get full path of an application available in current environment. Same as writing
        ///     its name in CMD.
        /// </summary>
        /// <param name="filename">
        ///     Name of the executable, including the extension
        /// </param>
        /// <returns>
        /// </returns>
        public static string GetFullPathOfExecutable(string filename)
        {
            IEnumerable<string> paths = new[] { Environment.CurrentDirectory };
            var pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (pathVariable != null)
            {
                paths = paths.Concat(pathVariable.Split(';'));
            }

            var combinations = paths.Select(x => Path.Combine(x, filename));
            return combinations.FirstOrDefault(File.Exists) ?? GetExecutablePathFromAppPaths(filename);
        }

        /// <summary>
        ///     Version of Path.Combine with much less restrictive input checks, and additional path cleanup.
        /// </summary>
        internal static string GenerousCombine(string path1, string path2)
        {
            if (path1 == null || path2 == null)
            {
                throw new ArgumentNullException(path1 == null ? nameof(path1) : nameof(path2));
            }

            path1 = NormalizePath(path1);
            path2 = NormalizePath(path2);

            if (path2.Length == 0)
            {
                return path1;
            }

            if (path1.Length == 0 || Path.IsPathRooted(path2))
            {
                return path2;
            }

            return path1 + Path.DirectorySeparatorChar + path2;
        }

        /// <summary>
        ///     Get full directory path of directory that contains the item pointed at by the path string.
        /// </summary>
        internal static string GetDirectory(string fullPath)
        {
            var trimmed = fullPath.TrimEnd('"', ' ', '\\').TrimStart('"', ' ');
            if (!trimmed.Contains('\\'))
            {
                return string.Empty;
            }

            var index = trimmed.LastIndexOf('\\');
            return index < trimmed.Length ? trimmed.Substring(0, index) : string.Empty;
        }

        /// <summary>
        ///     Get the topmost part of the path. If this is not a valid path return string.Empty.
        /// </summary>
        internal static string GetName(string fullPath)
        {
            var trimmed = fullPath.TrimEnd('"', ' ', '\\');
            if (!trimmed.Contains('\\'))
            {
                return string.Empty;
            }

            var index = trimmed.LastIndexOf('\\') + 1;
            return index < trimmed.Length ? trimmed.Substring(index) : string.Empty;
        }

        // Try to get the windows directory, returns null if failed
        internal static DirectoryInfo GetWindowsDirectory()
        {
            try
            {
                var windowsDirectory = Environment.GetEnvironmentVariable("SystemRoot");
                if (windowsDirectory != null)
                {
                    return new DirectoryInfo(windowsDirectory);
                }
            }
            catch
            {
                //Check other
            }
            try
            {
                var windowsDirectory = Environment.GetEnvironmentVariable("windir");
                if (windowsDirectory != null)
                {
                    return new DirectoryInfo(windowsDirectory);
                }
            }
            catch
            {
                //Messed up environment variables or security too high
            }
            return null;
        }

        /// <summary>
        ///     Remove unnecessary spaces, quotes and path separators from start and end of the
        ///     path. Might produce different path than intended in case it contains invalid unicode characters.
        /// </summary>
        internal static string NormalizePath(string path1)
        {
            if (path1 == null)
            {
                throw new ArgumentNullException(nameof(path1));
            }

            return path1.SafeNormalize().Trim(PathTrimChars);
        }

        internal static bool PathsEqual(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2))
            {
                return false;
            }

            try
            {
                path1 = path1.SafeNormalize().Trim(PathTrimChars);
                path2 = path2.SafeNormalize().Trim(PathTrimChars);
                return path1.Equals(path2, StringComparison.InvariantCultureIgnoreCase);
            }
            catch
            {
                // Fall back to ordinal in case SafeNormalize isn't safe enough
                return path1.Trim(PathTrimChars).Equals(path2.Trim(PathTrimChars), StringComparison.OrdinalIgnoreCase);
            }
        }

        internal static bool PathsEqual(FileSystemInfo path1, FileSystemInfo path2) => path1 != null && path2 != null && PathsEqual(path1.FullName, path2.FullName);

        /// <summary>
        ///     Change path to normal case. Example: C:\PROGRAM FILES =&gt; C:\Program files
        /// </summary>
        internal static string PathToNormalCase(string path)
        {
            var directoryParts = NormalizePath(path).Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (directoryParts.Length < 1)
            {
                return string.Empty;
            }

            var result = string.Empty;

            for (var i = 0; i < directoryParts.Length; i++)
            {
                var part = directoryParts[i].ToLower();
                result = $"{result}{part.Substring(0, 1).ToUpperInvariant() + part.Substring(1)}\\";
            }

            return result;
        }

        /// <summary>
        ///     Check if subPath is a sub path inside basePath. If isFilesystemPath is true then
        ///     attempt to normalize the path to its absolute form on the filesystem. Set to false
        ///     for registry and other paths.
        /// </summary>
        internal static bool SubPathIsInsideBasePath(string basePath, string subPath, bool normalizeFilesystemPath)
        {
            if (basePath == null)
            {
                return false;
            }

            basePath = NormalizePath(basePath).Replace('\\', '/');
            if (string.IsNullOrEmpty(basePath))
            {
                return false;
            }

            if (normalizeFilesystemPath)
            {
                try { basePath = Path.GetFullPath(basePath).Replace('\\', '/'); }
                catch (SystemException) { }
            }

            if (subPath == null)
            {
                return false;
            }

            subPath = NormalizePath(subPath).Replace('\\', '/');
            if (string.IsNullOrEmpty(subPath))
            {
                return false;
            }

            if (!normalizeFilesystemPath)
            {
                return subPath.StartsWith(basePath + '/', StringComparison.InvariantCultureIgnoreCase);
            }

            try { subPath = Path.GetFullPath(subPath).Replace('\\', '/'); }
            catch (SystemException) { }

            return subPath.StartsWith(basePath + '/', StringComparison.InvariantCultureIgnoreCase);
        }

        /// <param name="executableName">
        ///     name of the executable, including .exe
        /// </param>
        private static string GetExecutablePathFromAppPaths(string executableName)
        {
            const string appPaths = @"Software\Microsoft\Windows\CurrentVersion\App Paths";
            var executableEntry = Path.Combine(appPaths, executableName);
            using var key = Registry.CurrentUser.OpenSubKey(executableEntry) ?? Registry.LocalMachine.OpenSubKey(executableEntry);
            return key?.GetStringSafe(null);
        }
    }
}