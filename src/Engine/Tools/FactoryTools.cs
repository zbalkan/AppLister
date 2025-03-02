using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Engine.Extensions;

namespace Engine.Tools
{
    internal static class FactoryTools
    {
        internal static IEnumerable<Dictionary<string, string>> ExtractAppDataSetsFromHelperOutput(string helperOutput)
        {
            ICollection<string> allParts = helperOutput.SplitNewlines(StringSplitOptions.None);
            while (allParts.Count > 0)
            {
                var singleAppParts = allParts.TakeWhile(x => !string.IsNullOrEmpty(x)).ToList();
                allParts = allParts.Skip(singleAppParts.Count + 1).ToList();

                if (!singleAppParts.Any())
                {
                    continue;
                }

                yield return singleAppParts.Where(x => x.Contains(':')).ToDictionary(
                    x => x.Substring(0, x.IndexOf(":", StringComparison.Ordinal)).Trim(),
                    x => x.Substring(x.IndexOf(":", StringComparison.Ordinal) + 1).Trim());
            }
        }

        /// <summary>
        ///     Warning: only use with helpers that output unicode and use 0 as success return code.
        /// </summary>
        internal static string StartHelperAndReadOutput(string filename, string args)
        {
            if (!File.Exists(filename))
            {
                return null;
            }

            using var process = Process.Start(new ProcessStartInfo(filename, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.Unicode
            });
            try
            {
                var sw = Stopwatch.StartNew();
                var output = process?.StandardOutput.ReadToEnd();
                Debug.WriteLine($"[Performance] Running command {filename} {args} took {sw.ElapsedMilliseconds}ms");
                return process?.ExitCode == 0 ? output : null;
            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }
    }
}