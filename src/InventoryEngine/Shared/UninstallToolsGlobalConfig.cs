using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using InventoryEngine.Factory;
using InventoryEngine.Shared;
using InventoryEngine.Tools;

namespace InventoryEngine
{
    internal static class UninstallToolsGlobalConfig
    {
        internal static string AppInfoCachePath { get; }

        internal static bool AutoDetectCustomProgramFiles { get; set; }

        internal static bool AutoDetectScanRemovable { get; set; }

        /// <summary>
        ///     Custom "Program Files" directories. Use with dirs that get used to install
        ///     applications to.
        /// </summary>
        internal static string[] CustomProgramFiles { get; set; }

        /// <summary>
        ///     Automatize non-quiet uninstallers.
        /// </summary>
        internal static bool QuietAutomatization { get; set; }

        /// <summary>
        ///     Kill stuck automatized uninstallers.
        /// </summary>
        internal static bool QuietAutomatizationKillStuck { get; set; }

        internal static bool ScanChocolatey { get; set; } = true;

        internal static bool ScanDrives { get; set; } = true;

        internal static bool ScanOculus { get; set; } = true;

        internal static bool ScanPreDefined { get; set; } = true;

        internal static bool ScanRegistry { get; set; } = true;

        internal static bool ScanScoop { get; set; } = true;

        internal static bool ScanSteam { get; set; } = true;

        internal static bool ScanStoreApps { get; set; } = true;

        internal static bool ScanWinFeatures { get; set; } = true;

        internal static bool ScanWinUpdates { get; set; } = true;

        internal static bool UninstallerAutomatizerExists { get; }

        internal static string UninstallerAutomatizerPath { get; }

        internal static bool UseQuietUninstallDaemon { get; set; }

        internal static string AppLocation { get; }

        /// <summary>
        ///     Path to directory this assembly sits in.
        /// </summary>
        internal static string AssemblyLocation { get; }

        /// <summary>
        ///     Directory names that should be ignored for safety.
        /// </summary>
        internal static IEnumerable<string> DirectoryBlacklist { get; }

        /// <summary>
        ///     Directories that can contain program junk.
        /// </summary>
        internal static IEnumerable<string> JunkSearchDirs { get; }

        /// <summary>
        ///     Directory names that probably aren't top-level or contain applications.
        /// </summary>
        internal static IEnumerable<string> QuestionableDirectoryNames { get; }

        /// <summary>
        ///     Built-in program files paths.
        /// </summary>
        internal static IEnumerable<string> StockProgramFiles { get; }

        internal static string WindowsDirectory { get; }

        private static readonly string _pf64, _pf32;

        static UninstallToolsGlobalConfig()
        {
            AssemblyLocation = Assembly.GetExecutingAssembly().Location;
            if (new[] { ".dll", ".exe" }.Any(item => AssemblyLocation.Contains(item)))
            {
                AssemblyLocation = PathTools.GetDirectory(AssemblyLocation);
            }

            var dir = new DirectoryInfo(AssemblyLocation);
            if (dir.Name.StartsWith("win-x") && dir.Parent != null)
            {
                dir = dir.Parent;
            }

            AppLocation = dir.FullName;

            UninstallerAutomatizerPath = Path.Combine(AssemblyLocation, "UninstallerAutomatizer.exe");
            UninstallerAutomatizerExists = File.Exists(UninstallerAutomatizerPath);

            QuestionableDirectoryNames = new[]
            {
                "install", "settings", "config", "configuration", "users", "data"
            }.AsEnumerable();

            DirectoryBlacklist = new[]
            {
                "Microsoft", "Microsoft Games", "Temp", "Programs", "Common", "Common Files", "Clients",
                "Desktop", "Internet Explorer", "Windows", "Windows NT", "Windows Photo Viewer", "Windows Mail",
                "Windows Defender", "Windows Media Player", "Uninstall Information", "Reference Assemblies",
                "InstallShield Installation Information", "Installer", "winsxs", "WindowsApps", "DirectX", "DirectXRedist"
            }.AsEnumerable();

            WindowsDirectory = WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_WINDOWS);

            StockProgramFiles = new[]
            {
                WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_PROGRAM_FILES),
                WindowsTools.GetProgramFilesX86Path()
            }.Distinct().ToList().AsEnumerable();

            // JunkSearchDirs --------------
            var localData = WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_LOCAL_APPDATA);
            var paths = new List<string>
            {
                WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_PROGRAMS),
                WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_COMMON_PROGRAMS),
                WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_APPDATA),
                WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_COMMON_APPDATA),
                localData
            };

            var appDataParentDir = Path.GetDirectoryName(localData.TrimEnd('\\', '/', ' '));
            if (!string.IsNullOrEmpty(appDataParentDir))
            {
                var lowDir = Path.Combine(appDataParentDir, "LocalLow");
                if (Directory.Exists(lowDir))
                {
                    paths.Add(lowDir);
                }
            }

            var vsPath = Path.Combine(localData, "VirtualStore");
            if (Directory.Exists(vsPath))
            {
                paths.AddRange(Directory.GetDirectories(vsPath));
            }

            JunkSearchDirs = paths.Distinct().ToList().AsEnumerable();

            AppInfoCachePath = Path.Combine(AssemblyLocation, "InfoCache.xml");

            _pf32 = WindowsTools.GetProgramFilesX86Path();
            _pf64 = WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_PROGRAM_FILES);
            if (string.IsNullOrWhiteSpace(_pf64) || PathTools.PathsEqual(_pf32, _pf64))
            {
                _pf64 = null;
            }
        }

        /// <summary>
        ///     Check if the path is inside of 64 or 32 bit program files
        /// </summary>
        internal static MachineType IsPathInsideProgramFiles(string fullPath)
        {
            if (fullPath.StartsWith(_pf32, StringComparison.InvariantCultureIgnoreCase))
            {
                return MachineType.X86;
            }

            if (_pf64 != null && fullPath.StartsWith(_pf64, StringComparison.InvariantCultureIgnoreCase))
            {
                return MachineType.X64;
            }

            return MachineType.Unknown;
        }

        /// <summary>
        ///     Check if dir is a system directory and should be left alone.
        /// </summary>
        internal static bool IsSystemDirectory(DirectoryInfo dir) => (dir.Attributes & FileAttributes.System) == FileAttributes.System
                   || dir.FullName.StartsWith(WindowsDirectory, StringComparison.OrdinalIgnoreCase)
                   || DirectoryBlacklist.Any(y => y.Equals(dir.Name, StringComparison.InvariantCultureIgnoreCase));

        /// <summary>
        ///     Check if dir is a system directory and should be left alone.
        /// </summary>
        internal static bool IsSystemDirectory(string installLocation)
        {
            if (string.IsNullOrEmpty(installLocation))
            {
                return false;
            }

            try
            {
                return IsSystemDirectory(new DirectoryInfo(installLocation));
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine(ex);
                // Treat this as a no-touch directory just to be safe
                return true;
            }
            catch (IOException ex)
            {
                Trace.WriteLine(ex);
                // Treat this as a no-touch directory just to be safe
                return true;
            }
        }

        /// <summary>
        ///     Directiories containing programs, both built in "Program Files" and user-defined
        ///     ones. Fast.
        /// </summary>
        internal static IEnumerable<string> GetAllProgramFiles()
        {
            if (CustomProgramFiles == null || CustomProgramFiles.Length == 0)
            {
                return StockProgramFiles;
            }

            // Create copy of custom dirs in case they change
            return StockProgramFiles.Concat(CustomProgramFiles).ToList();
        }

        /// <summary>
        ///     Get a list of directiories containing programs. Optionally user-defined directories
        ///     are added. The boolean value is true if the directory is confirmed to contain 64bit
        ///     applications, false if 32bit.
        /// </summary>
        /// <param name="includeUserDirectories">
        ///     Add user-defined directories.
        /// </param>
        internal static List<DirectoryInfo> GetProgramFilesDirectories(bool includeUserDirectories)
        {
            var pfDirectories = new List<string>(2)
            {
                _pf32
            };
            if (_pf64 != null)
            {
                pfDirectories.Add(_pf64);
            }

            if (includeUserDirectories && CustomProgramFiles != null)
            {
                pfDirectories.AddRange(CustomProgramFiles.Where(x => !pfDirectories.Any(y => PathTools.PathsEqual(x, y))));
            }

            pfDirectories.Add(Path.Combine(WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_APPDATA), "Programs"));
            pfDirectories.Add(Path.Combine(WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_LOCAL_APPDATA), "Programs"));
            pfDirectories.Add(Path.Combine(WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_COMMON_APPDATA), "Programs"));

            var output = new List<DirectoryInfo>(pfDirectories.Count);
            foreach (var directory in pfDirectories)
            {
                // Ignore missing or inaccessible directories
                try
                {
                    var di = new DirectoryInfo(directory);
                    if (di.Exists)
                    {
                        output.Add(di);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Fail("Failed to open dir", ex.Message);
                }
            }

            return output;
        }
    }
}