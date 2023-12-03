using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Xml;
using InventoryEngine.Shared;
using InventoryEngine.Tools;

namespace InventoryEngine.Factory
{
    internal class StoreAppFactory : IIndependantUninstallerFactory
    {
        public string DisplayName => "Progress_AppStores_WinStore";

        private static readonly string appRoot = $"{WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_PROGRAM_FILES)}\\WindowsApps";
        private static readonly PowerShell powershellInstance = PowerShell.Create();

        private static readonly string windowsPath = WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_WINDOWS);
        public IReadOnlyList<ApplicationUninstallerEntry> GetUninstallerEntries()
        {
            var results = new List<ApplicationUninstallerEntry>();
            foreach (var appxPackage in GetAppxPackages())
            {
                results.Add(MapToEntry(appxPackage));
            }
            return results.AsReadOnly();
        }

        public bool IsEnabled() => UninstallToolsGlobalConfig.ScanStoreApps;

        private static DateTime FindCreationTime(string installLocation)
        {
            if (string.IsNullOrEmpty(installLocation))
            {
                return DateTime.MinValue;
            }

            try
            {
                return Directory.GetCreationTime(installLocation);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return DateTime.MinValue;
            }
        }

        private static string[] FindExecutables(PSObject manifest)
        {
            if (manifest == default)
            {
                return Array.Empty<string>();
            }

            var executableName = ResolveExecutableName(manifest);

            if (string.IsNullOrEmpty(executableName))
            {
                return Array.Empty<string>();
            }
            executableName = executableName.Split('\\').Last();

            return Directory.GetFiles(appRoot, executableName, SearchOption.AllDirectories).ToArray();
        }

        private static PSObject GetAppxManifest(string packageName)
        {
            PSObject manifest = default;
            powershellInstance.Commands.Clear();
            powershellInstance
                .AddCommand("Get-AppxPackageManifest")
                .AddParameter("Package", packageName);

            try
            {
                manifest = powershellInstance.Invoke()[0];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return manifest;
        }

        private static Collection<PSObject> GetAppxPackages()
        {
            powershellInstance.Commands.Clear();
            powershellInstance.AddCommand("Get-AppxPackage");
            return powershellInstance.Invoke();
        }

        private static ApplicationUninstallerEntry MapToEntry(PSObject appxPackage)
        {
            var props = appxPackage.Properties;
            var manifest = GetAppxManifest(props["PackageFullName"].Value.ToString());
            var fullName = ParseFullName(props);
            var installLocation = ParseInstallLocation(props);

            return new ApplicationUninstallerEntry()
            {
                CacheIdOverride = fullName,
                RatingId = fullName.Substring(0, fullName.IndexOf("_", StringComparison.Ordinal)),
                UninstallString = string.Empty,
                QuietUninstallString = string.Empty,
                RawDisplayName = ResolveDisplayName(props, manifest),
                Publisher = ResolvePublisherName(props, manifest),
                IsValid = true,
                UninstallerKind = UninstallerType.StoreApp,
                InstallLocation = installLocation,
                InstallDate = FindCreationTime(installLocation),
                IsProtected = ResolveProtected(props),
                SystemComponent = SetSystemComponent(installLocation),
                Is64Bit = ResolveArchitecture(props),
                SortedExecutables = FindExecutables(manifest),
                DisplayVersion = ResolveVersion(props)
            };
        }

        private static string ParseFullName(PSMemberInfoCollection<PSPropertyInfo> props)
        {
            var rawName = props["PackageFullName"].Value.ToString();
            var publisherId = props["PublisherId"].Value.ToString();
            return rawName.Replace($"__{publisherId}", string.Empty);
        }

        private static string ParseInstallLocation(PSMemberInfoCollection<PSPropertyInfo> props)
        {
            string installLocation;
            try
            {
                var il = props.SingleOrDefault(p => p.Name.Equals("InstallLocation"));
                if (il == null)
                {
                    return string.Empty;
                }
                installLocation = il.Value as string;
                if (string.IsNullOrEmpty(installLocation))
                {
                    installLocation = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                installLocation = string.Empty;
            }

            return installLocation;
        }

        private static MachineType ResolveArchitecture(PSMemberInfoCollection<PSPropertyInfo> props)
        {
            switch (props["Architecture"].Value.ToString())
            {
                case "X64":
                    return MachineType.X64;
                case "X86":
                    return MachineType.X86;
                case "Neutral":
                default:
                    return MachineType.Unknown;
            }
        }

        private static string ResolveDisplayName(PSMemberInfoCollection<PSPropertyInfo> props, PSObject manifest)
        {
            string displayName;
            if (manifest == default)
            {
                displayName = props["Name"].Value.ToString();
            }
            else
            {
                var package = manifest.Members["Package"].Value as XmlElement;
                displayName = package["Properties"]["DisplayName"].InnerText;

                if (displayName.StartsWith("ms-resource"))
                {
                    var identity = package["Identity"];
                    displayName = identity.Attributes["Name"].Value;
                }
            }

            if (string.IsNullOrEmpty(displayName))
            {
                throw new Exception();
            }
            return displayName;
        }

        private static string ResolveExecutableName(PSObject manifest)
        {
            if (manifest == default)
            {
                return string.Empty;
            }

            var package = manifest.Members["Package"].Value as XmlElement;

            try
            {
                var applications = package["Applications"];
                if (applications == null)
                {
                    return string.Empty;
                }
                var application = applications["Application"];
                var exec = application.Attributes["Executable"];
                if (exec == null)
                {
                    return string.Empty;
                }
                return exec.InnerText;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool ResolveProtected(PSMemberInfoCollection<PSPropertyInfo> props) => props["SignatureKind"].Value.ToString().Equals("System");

        private static string ResolvePublisherDisplayName(PSObject manifest)
        {
            if (manifest == default)
            {
                return string.Empty;
            }

            var package = manifest.Members["Package"].Value as XmlElement;
            return package["Properties"]["PublisherDisplayName"].InnerText;
        }

        private static string ResolvePublisherName(PSMemberInfoCollection<PSPropertyInfo> props, PSObject manifest)
        {
            var raw = props["Publisher"].Value.ToString();
            var firstComma = raw.IndexOf(',');
            string newName;
            if (firstComma == -1)
            {
                newName = raw.Replace("CN=", string.Empty);
            }
            else
            {
                newName = raw.Substring(0, firstComma).Replace("CN=", string.Empty);
            }

            var isValidGuid = Guid.TryParse(newName, out _);
            if (isValidGuid)
            {
                newName = ResolvePublisherDisplayName(manifest);
            }

            return newName;
        }
        private static string ResolveVersion(PSMemberInfoCollection<PSPropertyInfo> props) => props["Version"].Value.ToString();

        private static bool SetSystemComponent(string installLocation)
        {
            if (string.IsNullOrEmpty(installLocation))
            {
                return false;
            }

            return installLocation.StartsWith(windowsPath, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}