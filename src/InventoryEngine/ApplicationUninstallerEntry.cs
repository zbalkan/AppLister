using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InventoryEngine.Extensions;
using InventoryEngine.Startup;
using InventoryEngine.Tools;

namespace InventoryEngine
{
    public class ApplicationUninstallerEntry
    {
        public string AboutUrl { get; set; }

        /// <summary>
        ///     Product code used by msiexec. If it wasn't found, returns Guid.Empty.
        /// </summary>
        public Guid BundleProviderKey { get; set; }

        public string CacheIdOverride { get; set; }

        public string Comment { get; set; }

        public string DisplayName
        {
            get { return (string.IsNullOrEmpty(RawDisplayName) ? RegistryKeyName : RawDisplayName) ?? string.Empty; }
            set { RawDisplayName = value; }
        }

        public string DisplayNameTrimmed => DisplayName.StripStringFromVersionNumber();

        public string DisplayVersion { get; set; }

        public bool HasStartups => StartupEntries?.Any() == true;

        public DateTime InstallDate { get; set; }

        public string InstallLocation
        {
            get { return _installLocation; }
            set { _installLocation = CleanupPath(value); }
        }

        public string InstallSource
        {
            get { return _installSource; }
            set { _installSource = CleanupPath(value); }
        }

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
        ///     The application's uniunstaller is mentioned in the registry (if it's not normal
        ///     uninstallers will not see it)
        /// </summary>
        public bool IsRegistered { get; set; }

        /// <summary>
        ///     True if this is an update for another product
        /// </summary>
        public bool IsUpdate { get; set; }

        /// <summary>
        ///     True if the application can be uninstalled. False if the uninstaller is missing or
        ///     is otherwise invalid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        ///     True if the application is listed as a web browser.
        /// </summary>
        public bool IsWebBrowser { get; set; }

        public string ModifyPath
        {
            get { return _modifyPath; }
            set { _modifyPath = CleanupPath(value); }
        }

        public string ParentKeyName { get; set; }

        public string Publisher { get; set; }

        public string PublisherTrimmed => string.IsNullOrEmpty(Publisher)
                    ? string.Empty
                    : Publisher.Replace("(R)", string.Empty)
                        .ExtendedTrimEndAny(CompanyNameEndTrimmers, StringComparison.CurrentCultureIgnoreCase);

        public bool QuietUninstallPossible => !string.IsNullOrEmpty(QuietUninstallString) ||
                                                      (UninstallerKind == UninstallerType.Msiexec &&
                                                       BundleProviderKey != Guid.Empty);

        public string QuietUninstallString { get; set; }

        public string RatingId
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

        public string RegistryKeyName { get; set; }

        /// <summary>
        ///     Full registry path of this entry
        /// </summary>
        public string RegistryPath { get; set; }

        public IEnumerable<StartupEntryBase> StartupEntries { get; set; }

        public bool SystemComponent { get; set; }

        public string UninstallerFullFilename
        {
            get { return _uninstallerFullFilename; }
            set
            {
                _uninstallerFullFilename = value;

                UninstallerLocation = ApplicationEntryTools.ExtractDirectoryName(UninstallerFullFilename)
                                      ?? UninstallerLocation ?? string.Empty;
            }
        }

        public UninstallerType UninstallerKind { get; set; }

        public string UninstallerLocation { get; set; }

        public bool UninstallPossible => !string.IsNullOrEmpty(UninstallString);

        //public bool IsInstalled { get; internal set; }
        public string UninstallString
        {
            get { return _uninstallString; }
            set
            {
                _uninstallString = value;

                UninstallerFullFilename = ApplicationEntryTools.ExtractFullFilename(value)
                    ?? UninstallerFullFilename ?? string.Empty;
            }
        }

        internal string RawDisplayName { get; set; }

        /// <summary>
        ///     Ordered collection of filenames that could be the main executable of the
        ///     application. The most likely files are first, the least likely are last.
        /// </summary>
        internal string[] SortedExecutables { get; set; }

        private static readonly IEnumerable<string> CompanyNameEndTrimmers =
                    new[] { "corp", "corporation", "corporations", "limited", "inc", "incorporated", "ltd", "foundation", "s.r.o" };

        /// <summary>
        ///     List of properties that migh have changed by updating the key property
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

        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        private string _installLocation;
        private string _installSource;
        private string _modifyPath;
        private string _ratingId;
        private string _uninstallerFullFilename;
        private string _uninstallString;

        /// <summary>
        ///     Get a unique cache ID of this item. Returns null if there isn't enough information
        ///     to get a reasonably unique key.
        /// </summary>
        public string GetCacheId()
        {
            if (!string.IsNullOrEmpty(CacheIdOverride))
            {
                return CacheIdOverride;
            }

            var rid = RatingId;
            if (!string.IsNullOrEmpty(rid))
            {
                return rid;
            }

            if (!string.IsNullOrEmpty(DisplayName) && !string.IsNullOrEmpty(InstallLocation))
            {
                return DisplayName + InstallLocation;// + DisplayVersion + InstallDate + EstimatedSize;
            }

            return null;
        }

        public IEnumerable<string> GetSortedExecutables()
        {
            if (SortedExecutables == null)
            {
                return Enumerable.Empty<string>();
            }

            var output = SortedExecutables.AsEnumerable();
            if (!string.IsNullOrEmpty(UninstallerFullFilename))
            {
                output = output.OrderBy(x => x.Equals(UninstallerFullFilename, StringComparison.InvariantCultureIgnoreCase));
            }

            return output;
        }

        /// <summary>
        ///     Check if the install location is not empty and is not a system directory
        /// </summary>
        public bool IsInstallLocationValid()
        {
            if (string.IsNullOrEmpty(InstallLocation?.Trim()))
            {
                return false;
            }

            return !UninstallToolsGlobalConfig.GetAllProgramFiles().Any(x => PathTools.PathsEqual(x, InstallLocation));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(DisplayName)
                .AppendFormat(" | {0}", Publisher)
                .AppendFormat(" | {0}", DisplayVersion)
                .AppendFormat(" | {0}", UninstallString)
                .AppendFormat(" | {0}", Comment);

            return sb.ToString();
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