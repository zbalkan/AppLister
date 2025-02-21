using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using InventoryEngine.Extensions;
using InventoryEngine.Shared;
using Microsoft.Win32;

namespace InventoryEngine.Tools
{
    internal static partial class WindowsTools
    {
        private static readonly IEnumerable<string> LibraryTypes = new[]
                {
            "DLL",
            "SYS"
        };

        private static readonly IEnumerable<string> SystemExecutableTypes = new[]
                {
            "BAT",
            "BIN",
            "CMD",
            "COM",
            "CPL",
            "EXE",
            "GADGET",
            "INF1",
            "INS",
            "INX",
            "ISU",
            "JOB",
            "JSE",
            "LNK",
            "MSC",
            "MSI",
            "MSP",
            "MST",
            "PAF",
            "PIF",
            "PS1",
            "REG",
            "RGS",
            "SCT",
            "SHB",
            "SHS",
            "U3P",
            "VB",
            "VBE",
            "VBS",
            "VBSCRIPT",
            "WS",
            "WSF"
        };

        private static readonly IEnumerable<string> ThirdPartyExecutableTypes = new[]
                {
            "0XE",
            "73K",
            "89K",
            "A6P",
            "AC",
            "ACC",
            "ACR",
            "ACTM",
            "AHK",
            "AIR",
            "APP",
            "ARSCRIPT",
            "AS",
            "ASB",
            "AWK",
            "AZW2",
            "BEAM",
            "BTM",
            "CEL",
            "CELX",
            "CHM",
            "COF",
            "CRT",
            "DEK",
            "DLD",
            "DMC",
            "DOCM",
            "DOTM",
            "DXL",
            "EAR",
            "EBM",
            "EBS",
            "EBS2",
            "ECF",
            "EHAM",
            "ELF",
            "ES",
            "EX4",
            "EXOPC",
            "EZS",
            "FAS",
            "FKY",
            "FPI",
            "FRS",
            "FXP",
            "GS",
            "HAM",
            "HMS",
            "HPF",
            "HTA",
            "IIM",
            "IPF",
            "ISP",
            "JAR",
            "JS",
            "JSX",
            "KIX",
            "LO",
            "LS",
            "MAM",
            "MCR",
            "MEL",
            "MPX",
            "MRC",
            "MS",
            "MS",
            "MXE",
            "NEXE",
            "OBS",
            "ORE",
            "OTM",
            "PEX",
            "PLX",
            "POTM",
            "PPAM",
            "PPSM",
            "PPTM",
            "PRC",
            "PVD",
            "PWC",
            "PYC",
            "PYO",
            "QPX",
            "RBX",
            "ROX",
            "RPJ",
            "S2A",
            "SBS",
            "SCA",
            "SCAR",
            "SCB",
            "SCR",
            "SCRIPT",
            "SMM",
            "SPR",
            "TCP",
            "THM",
            "TLB",
            "TMS",
            "UDF",
            "UPX",
            "URL",
            "VLX",
            "VPM",
            "WCM",
            "WIDGET",
            "WIZ",
            "WPK",
            "WPM",
            "XAP",
            "XBAP",
            "XLAM",
            "XLM",
            "XLSM",
            "XLTM",
            "XQT",
            "XYS",
            "ZL9"
        };

        internal static string GetEnvironmentPath(Csidl target)
        {
            var path = new StringBuilder(260);
            NativeMethods.SHGetSpecialFolderPath(IntPtr.Zero, path, (int)target, false);
            return path.ToString();
        }

        /// <summary>
        ///     Returns executable paths of all installed web browsers
        /// </summary>
        internal static IEnumerable<string> GetInstalledWebBrowsers()
        {
            // Check for built-in browsers that obviously don't have to conform to standards
            var results = new List<string>(new[]
            {
                Path.Combine(GetProgramFilesX86Path(), @"Internet Explorer\iexplore.exe"),
                Path.Combine(GetEnvironmentPath(Csidl.CSIDL_PROGRAM_FILES), @"Internet Explorer\iexplore.exe"),
                Path.Combine(GetEnvironmentPath(Csidl.CSIDL_WINDOWS), @"SystemApps\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\MicrosoftEdgeCP.exe"),
                Path.Combine(GetEnvironmentPath(Csidl.CSIDL_WINDOWS), @"SystemApps\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\MicrosoftEdge.exe")
            }.Where(File.Exists));

            // Check for 3rd party browsers in standard reg keys
            foreach (var internetKeyName in new[]
            {
                @"SOFTWARE\Clients\StartMenuInternet",
                @"SOFTWARE\WOW6432Node\Clients\StartMenuInternet"
            })
            {
                using var key = Registry.LocalMachine.OpenSubKey(internetKeyName);
                if (key == null)
                {
                    continue;
                }

                foreach (var registryKey in key.GetSubKeyNames())
                {
                    using var commandKey = key.OpenSubKey(registryKey + @"\shell\open\command");
                    var path = commandKey?.GetStringSafe(null);
                    if (path != null)
                    {
                        results.Add(path.Trim('\"'));
                    }
                }
            }

            return results.Distinct((x, y) => x.Equals(y, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        ///     Return 32 bit program files directory.
        /// </summary>
        internal static string GetProgramFilesX86Path()
        {
            var result = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            return string.IsNullOrEmpty(result) ? GetEnvironmentPath(Csidl.CSIDL_PROGRAM_FILES) : result;
        }

        internal static SecurityIdentifier GetUserSid() => WindowsIdentity.GetCurrent().User;

        /// <summary>
        ///     Check if the file can be executed and optionally if it's a library. Only the string
        ///     is compared, the path or file doesn't have to exist.
        /// </summary>
        /// <param name="filename">
        ///     Path containing the file name, it must contain the extension. The file doesn't have
        ///     to exist.
        /// </param>
        /// <param name="onlySystemTypes">
        ///     Should file types executed by third party applications be included?
        /// </param>
        /// <param name="includeLibraries">
        ///     Should library file types be included in the comparison?
        /// </param>
        internal static bool IsExecutable(string filename, bool onlySystemTypes, bool includeLibraries)
        {
            filename = filename.ExtendedTrimEndAny(new[] { "'", "\"" }, StringComparison.CurrentCultureIgnoreCase);

            if (includeLibraries &&
                LibraryTypes.Any(x => filename.EndsWith(x, StringComparison.CurrentCultureIgnoreCase)))
            {
                return true;
            }

            if (SystemExecutableTypes.Any(x => filename.EndsWith(x, StringComparison.CurrentCultureIgnoreCase)))
            {
                return true;
            }

            if (!onlySystemTypes &&
                ThirdPartyExecutableTypes.Any(x => filename.EndsWith(x, StringComparison.CurrentCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        internal static string ResolveShortcut(string filename)
        {
            var link = new NativeMethods.ShellLink();

            // ReSharper disable once SuspiciousTypeConversion.Global
            ((NativeMethods.IPersistFile)link).Load(filename, NativeMethods.StgmRead);
            var sb = new StringBuilder(NativeMethods.MaxPath);
            var data = new NativeMethods.Win32FindDataw();

            // ReSharper disable once SuspiciousTypeConversion.Global
            ((NativeMethods.IShellLinkW)link).GetPath(sb, sb.Capacity, ref data, 0);
            return sb.ToString();
        }
    }
}