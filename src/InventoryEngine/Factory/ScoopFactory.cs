using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using InventoryEngine.InfoAdders;
using InventoryEngine.Junk.Confidence;
using InventoryEngine.Junk.Containers;
using InventoryEngine.Shared;
using InventoryEngine.Tools;

namespace InventoryEngine.Factory
{
    internal sealed partial class ScoopFactory : IIndependantUninstallerFactory
    {
        public string DisplayName => "Progress_AppStores_Scoop";

        private static readonly JsonSerializerOptions _jsonOptions;
        private static string _powershellPath;
        private static string _scoopGlobalPath;
        private static string _scoopUserPath;
        private static string _scriptPath;

        static ScoopFactory()
        {
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web); // ignore property name case
            _jsonOptions.Converters.Add(new PowerShellDateTimeOffsetConverter());
        }

        public static ApplicationUninstallerEntry CreateUninstallerEntry(
            string name,
            string version,
            bool isGlobal,
            AppExecutablesSearcher searcher)
        {
            var entry = new ApplicationUninstallerEntry
            {
                RawDisplayName = name,
                DisplayVersion = ApplicationEntryTools.CleanupDisplayVersion(version),
                RatingId = "Scoop " + name
            };

            var installDir = Path.Combine(isGlobal ? _scoopGlobalPath : _scoopUserPath, "apps\\" + name);
            if (Directory.Exists(installDir))
            {
                var executables = new List<string>();
                var currentDir = Path.Combine(installDir, "current");

                try
                {
                    var install = JsonDocument.Parse(File.ReadAllText(Path.Combine(currentDir, "install.json")))
                        .Deserialize(typeof(AppInstall), _jsonOptions) as AppInstall;

                    var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(currentDir, "manifest.json")))
                        .Deserialize(typeof(AppManifest), options: _jsonOptions) as AppManifest;

                    entry.AboutUrl = manifest.Homepage;

                    var shortcuts = manifest.Architecture?[install.Architecture]?.Shortcuts ?? manifest.Shortcuts;
                    if (shortcuts != null)
                    {
                        var files = shortcuts.Select(x => Path.Combine(currentDir, x[0]))
                                             .Where(File.Exists)
                                             .Select(Path.GetFullPath)
                                             .Distinct(StringComparer.OrdinalIgnoreCase)
                                             .ToList();

                        executables.AddRange(files.Where(x => x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).Concat(files.Where(x => x.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))));
                    }

                    var bin = manifest.Architecture?[install.Architecture]?.Bin ?? manifest.Bin;
                    if (bin != null)
                    {
                        var filteredBins = bin.Select(x => Path.Combine(installDir, "current", x))
                                              .Where(File.Exists)
                                              .Select(Path.GetFullPath)
                                              .Except(executables, StringComparer.OrdinalIgnoreCase)
                                              .Where(x => x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
                                              .ToList();
                        executables.AddRange(filteredBins);
                    }

                    var env = manifest.Architecture?[install.Architecture]?.EnvAddPath ?? manifest.EnvAddPath;
                    if (env != null)
                    {
                        currentDir = Path.Combine(currentDir, env[0]);
                    }
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine(ex);
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine(ex);
                }

                if (executables.Any())
                {
                    // No need to sort, safe to assume the manifest has the most important executables in first positions
                    entry.SortedExecutables = executables.ToArray();
                }
                else
                {
                    // Avoid looking for executables in old versions
                    entry.InstallLocation = currentDir;
                    searcher.AddMissingInformation(entry);
                }

                entry.InstallLocation = installDir;
            }

            entry.UninstallerKind = UninstallerType.PowerShell;
            entry.UninstallString = MakeScoopCommand("uninstall " + name + (isGlobal ? " --global" : "")).ToString();

            return entry;
        }

        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            var results = new List<ApplicationUninstallerEntry>();
            if (!GetScoopInfo())
            {
                return results;
            }

            // Make uninstaller for scoop itself
            var scoopEntry = new ApplicationUninstallerEntry
            {
                RawDisplayName = "Scoop",
                Comment = "Automated program installer",
                AboutUrl = "https://github.com/ScoopInstaller/Scoop",
                InstallLocation = _scoopUserPath,
                IsOrphaned = false,
                RatingId = "Scoop"
            };

            // Make sure the global directory gets removed as well
            var junk = new FileSystemJunk(new DirectoryInfo(_scoopGlobalPath), scoopEntry, null);
            junk.Confidence.Add(ConfidenceRecords.ExplicitConnection);
            junk.Confidence.Add(4);
            scoopEntry.AdditionalJunk.Add(junk);

            scoopEntry.UninstallString = MakeScoopCommand("uninstall scoop").ToString();
            scoopEntry.UninstallerKind = UninstallerType.PowerShell;
            results.Add(scoopEntry);

            // Make uninstallers for apps installed by scoop
            var result = RunScoopCommand("export");
            if (string.IsNullOrEmpty(result))
            {
                return results;
            }

            var exeSearcher = new AppExecutablesSearcher();

            // JSON export format since July 2022
            try
            {
                var export = JsonDocument.Parse(result)
                    .Deserialize(typeof(ExportInfo), _jsonOptions) as ExportInfo;

                foreach (var app in export.Apps)
                {
                    var entry = CreateUninstallerEntry(
                        app.Name, app.Version, app.IsPublic, exeSearcher);

                    entry.InstallDate = app.Updated.LocalDateTime;

                    results.Add(entry);
                }
            }
            // Fallback to plain text export format
            catch (JsonException e)
            {
                Debug.WriteLine("Fallback to plain text export format");

                foreach (var str in result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
                {
                    // Format should be "$app (v:$ver) $global_display $bucket $arch"
                    // app has no spaces, $global_display is *global*, bucket is inside [] brackets like [main]
                    // version should always be there but the check errored out for some users, everything after version is optional
                    string name;
                    string version = null;
                    var isGlobal = false;
                    var spaceIndex = str.IndexOf(" ", StringComparison.Ordinal);
                    if (spaceIndex > 0)
                    {
                        name = str.Substring(0, spaceIndex);

                        var startIndex = str.IndexOf("(v:", StringComparison.Ordinal);
                        if (startIndex > 0)
                        {
                            var verEndIndex = str.IndexOf(')', startIndex);
                            version = str.Substring(Math.Min(startIndex + 3, str.Length - 1), Math.Max(verEndIndex - startIndex - 3, 0));
                            if (version.Length == 0)
                            {
                                version = null;
                            }
                        }
                        isGlobal = str.Substring(spaceIndex).Contains("*global*");
                    }
                    else
                    {
                        name = str;
                    }

                    // Make sure that this isn't just a corrupted json export
                    if (string.Equals(name, "\"apps\":", StringComparison.Ordinal) ||
                        string.Equals(name, "\"buckets\":", StringComparison.Ordinal))
                    {
                        throw new InvalidDataException("Scoop export is in unkown or invalid format! Try updating Scoop and try again.\n\nContents:\n" + result, e);
                    }

                    var entry = CreateUninstallerEntry(name, version, isGlobal, exeSearcher);

                    results.Add(entry);
                }
            }

            return results;
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanScoop;

        private static bool GetScoopInfo()
        {
            try
            {
                _scoopUserPath = Environment.GetEnvironmentVariable("SCOOP");
                if (string.IsNullOrEmpty(_scoopUserPath))
                {
                    _scoopUserPath = Path.Combine(WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_PROFILE), "scoop");
                }

                _scoopGlobalPath = Environment.GetEnvironmentVariable("SCOOP_GLOBAL");
                if (string.IsNullOrEmpty(_scoopGlobalPath))
                {
                    _scoopGlobalPath = Path.Combine(WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_COMMON_APPDATA), "scoop");
                }

                _scriptPath = Path.Combine(_scoopUserPath, "shims\\scoop.ps1");

                if (File.Exists(_scriptPath))
                {
                    _powershellPath = PathTools.GetFullPathOfExecutable("powershell.exe");
                    if (!File.Exists(_powershellPath))
                    {
                        throw new InvalidOperationException("Detected Scoop program installer, but failed to detect PowerShell");
                    }

                    return true;
                }
            }
            catch (SystemException ex)
            {
                Debug.WriteLine("Failed to get Scoop info: " + ex);
            }

            return false;
        }

        private static ProcessStartCommand MakeScoopCommand(string scoopArgs) => new ProcessStartCommand(_powershellPath, $"-NoProfile -ex unrestricted \"{_scriptPath}\" {scoopArgs}");

        private static string RunScoopCommand(string scoopArgs)
        {
            var startInfo = MakeScoopCommand(scoopArgs).ToProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = false;
            startInfo.CreateNoWindow = true;
            startInfo.StandardOutputEncoding = Encoding.Default;

            using (var process = Process.Start(startInfo))
            {
                var sw = Stopwatch.StartNew();
                var output = process?.StandardOutput.ReadToEnd();
                Debug.WriteLine($"[Performance] Running command {startInfo.FileName} {startInfo.Arguments} took {sw.ElapsedMilliseconds}ms");
                return output;
            }
        }
    }
}