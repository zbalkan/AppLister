using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Engine.Extensions;
using Engine.Shared;
using Engine.Startup;
using Engine.Tools;

namespace Engine
{
    public class ApplicationUninstallerEntry
    {
        /// <summary>
        /// URL for more information about the application, such as its homepage or support site.
        /// Used to provide users with a direct reference for help or details.
        /// </summary>
        public string AboutUrl { get; set; }

        /// <summary>
        /// Additional notes or context about the application, such as install source or special status.
        /// Enables display of extra information to the user or for diagnostics.
        /// </summary>
        public string Comment { get; set; }

        public string DisplayName
        {
            get { return (string.IsNullOrEmpty(RawDisplayName) ? RegistryKeyName : RawDisplayName) ?? string.Empty; }
            set { RawDisplayName = value; }
        }

        /// <summary>
        /// Provides a cleaned-up version of the display name, omitting version numbers and extraneous text.
        /// Useful for grouping or comparing applications by name only.
        /// </summary>
        public string DisplayNameTrimmed => DisplayName.StripStringFromVersionNumber();

        /// <summary>
        /// The version of the application as presented to the user.
        /// Used for display, comparison, and update checks.
        /// </summary>
        public string DisplayVersion { get; set; }

        /// <summary>
        /// The date the application was installed.
        /// Used for sorting, filtering, or auditing installed software.
        /// </summary>
        public DateTime InstallDate { get; set; }

        /// <summary>
        /// Indicates the processor architecture (e.g., x86, x64) the application is intended for.
        /// Helps determine compatibility and for filtering in 32/64-bit environments.
        /// </summary>
        public MachineType Is64Bit { get; set; }

        /// <summary>
        ///     The application is present on the drive, but not in any of the application listings
        /// </summary>
        public bool IsOrphaned { get; set; }

        /// <summary>
        ///     Protection from uninstalling.
        /// </summary>
        public bool IsProtected { get; set; }

        /// <summary>
        ///     The application's uninstaller is mentioned in the registry (if it's not normal
        ///     uninstallers will not see it)
        /// </summary>
        public bool IsRegistered { get; set; }

        /// <summary>
        ///     True if this is an update for another product
        /// </summary>
        public bool IsUpdate { get; set; }

        /// <summary>
        ///     True if the application is listed as a web browser.
        /// </summary>
        public bool IsWebBrowser { get; set; }

        public string Publisher => string.IsNullOrEmpty(RawPublisher)
                            ? string.Empty
                            : RawPublisher.Replace("(R)", string.Empty)
                                .ExtendedTrimEndAny(CompanyNameEndTrimmers, StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Collection of startup entries associated with this application.
        /// Used to identify auto-starting components for management or cleanup.
        /// </summary>
        public IEnumerable<StartupEntryBase> StartupEntries { get; set; }

        /// <summary>
        /// Indicates if the application is a system component.
        /// Used to hide or protect critical system software from accidental removal.
        /// </summary>
        public bool SystemComponent { get; set; }

        /// <summary>
        /// True if this entry represents a device driver rather than a traditional application.
        /// Allows for specialized handling and filtering of drivers in the UI or logic.
        /// </summary>
        public bool IsDriver { get; set; }

        /// <summary>
        /// Specifies the type of uninstaller mechanism used (e.g., MSI, InnoSetup).
        /// Enables correct invocation and handling of the uninstall process.
        /// </summary>
        public UninstallerType UninstallerKind { get; set; }

        /// <summary>
        ///     Product code used by msiexec. If it wasn't found, returns Guid.Empty.
        /// </summary>
        internal Guid BundleProviderKey { get; set; }

        internal string InstallLocation
        {
            get { return _installLocation; }
            set { _installLocation = CleanupPath(value); }
        }

        internal string InstallSource
        {
            get { return _installSource; }
            set { _installSource = CleanupPath(value); }
        }
        /// <summary>
        ///     True if the application can be uninstalled. False if the uninstaller is missing or
        ///     is otherwise invalid.
        /// </summary>
        internal bool IsValid { get; set; }
        internal string ParentKeyName { get; set; }

        internal string RatingId
        {
            get
            {
                if (!string.IsNullOrEmpty(_ratingId))
                {
                    return _ratingId;
                }

                if (!string.IsNullOrEmpty(RegistryKeyName))
                {
                    return RegistryKeyName;
                }
                return null;
            }
            set { _ratingId = value; }
        }

        internal string RawDisplayName { get; set; }

        internal string RawPublisher { get; set; }
        internal string RegistryKeyName { get; set; }

        /// <summary>
        ///     Full registry path of this entry
        /// </summary>
        internal string RegistryPath { get; set; }
        /// <summary>
        ///     Ordered collection of filenames that could be the main executable of the
        ///     application. The most likely files are first, the least likely are last.
        /// </summary>
        internal string[] SortedExecutables { get; set; }

        internal string UninstallerFullFilename
        {
            get { return _uninstallerFullFilename; }
            set
            {
                _uninstallerFullFilename = value;

                UninstallerLocation = ApplicationEntryTools.ExtractDirectoryName(UninstallerFullFilename)
                                      ?? UninstallerLocation ?? string.Empty;
            }
        }
        internal string UninstallerLocation { get; set; }

        internal bool UninstallPossible => !string.IsNullOrEmpty(UninstallString);

        internal string UninstallString
        {
            get { return _uninstallString; }
            set
            {
                _uninstallString = value;

                UninstallerFullFilename = ApplicationEntryTools.ExtractFullFilename(value)
                    ?? UninstallerFullFilename ?? string.Empty;
            }
        }
        /// <summary>
        ///     List of properties that might have changed by updating the key property
        ///     IMPORTANT: Keep up to date!
        /// </summary>
        internal static readonly ILookup<string, string> PropertyRelationships = new Dictionary<string, List<string>>
                {
            {
                nameof(UninstallString),
                new List<string>
                {
                    nameof(UninstallerLocation),
                    nameof(UninstallerFullFilename)
                }
            },
            {
                nameof(UninstallerFullFilename),
                new List<string> {nameof(UninstallerLocation)}
            },
            {
                nameof(RawDisplayName),
                new List<string> {nameof(DisplayName)}
            },
            {
                nameof(RegistryKeyName),
                new List<string> {nameof(RatingId)}
            },
                }.SelectMany(x => x.Value.Select(y => new { x.Key, Value = y })).ToLookup(x => x.Key, x => x.Value);

        /// <summary>
        ///     Junk specified during creation of the entry that would not be detected afterwards.
        ///     It's added to the results during junk scan.
        /// </summary>
        internal readonly List<Junk.Containers.IJunkResult> AdditionalJunk = new List<Junk.Containers.IJunkResult>();

        private static readonly IEnumerable<string> CompanyNameEndTrimmers =
                            new[] { "corp", "corporation", "corporations", "limited", "inc", "incorporated", "ltd", "foundation", "s.r.o" };

        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        private string _installLocation;

        private string _installSource;

        private string _ratingId;

        private string _uninstallerFullFilename;

        private string _uninstallString;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(DisplayName)
                .AppendFormat(" | {0}", RawPublisher)
                .AppendFormat(" | {0}", DisplayVersion)
                .AppendFormat(" | {0}", UninstallString)
                .AppendFormat(" | {0}", Comment);

            return sb.ToString();
        }

        /// <summary>
        ///     Check if the install location is not empty and is not a system directory
        /// </summary>
        internal bool IsInstallLocationValid()
        {
            if (string.IsNullOrEmpty(InstallLocation?.Trim()))
            {
                return false;
            }

            return !UninstallToolsGlobalConfig.GetAllProgramFiles().Any(x => PathTools.PathsEqual(x, InstallLocation));
        }
        private static string CleanupPath(string path, bool isFilename = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (!isFilename)
            {
                // Try the fast method first for directories
                var trimmed = path.Trim('"', ' ', '\'', '\\', '/');

                if (!trimmed.ContainsAny(InvalidPathChars))
                {
                    return trimmed;
                }
            }

            try
            {
                path = ProcessTools.SeparateArgsFromCommand(path).FileName;
                if (!isFilename && path.Contains('.') && !Directory.Exists(path))
                {
                    return Path.GetDirectoryName(path);
                }
            }
            catch
            {
                // If sanitization failed just leave it be, it will be handled afterwards
            }
            return path.TrimEnd('\\');
        }
    }
}