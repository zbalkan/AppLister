using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Xml;
using InventoryEngine.Shared;
using InventoryEngine.Tools;

namespace InventoryEngine.Factory
{
    internal class StoreAppFactory : IIndependentUninstallerFactory
    {
        public string DisplayName => "Progress_AppStores_WinStore";

        private static readonly PowerShell PowershellInstance = PowerShell.Create();

        private static readonly string WindowsPath = WindowsTools.GetEnvironmentPath(Csidl.CSIDL_WINDOWS);

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
            if (manifest == null)
            {
                return Array.Empty<string>();
            }

            var executableName = ResolveExecutableName(manifest);

            if (string.IsNullOrEmpty(executableName))
            {
                return Array.Empty<string>();
            }
            return new[] { executableName };
        }

        private static PSObject GetAppxManifest(string packageName)
        {
            PSObject manifest = null;
            PowershellInstance.Commands.Clear();
            PowershellInstance
                .AddCommand("Get-AppxPackageManifest")
                .AddParameter("Package", packageName);

            try
            {
                manifest = PowershellInstance.Invoke()[0];
            }
            catch (CmdletInvocationException)
            {
                var message = $"Failed to call Get-AppxPackageManifest for package {packageName}";
                Debug.WriteLine(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return manifest;
        }

        private static Collection<PSObject> GetAppxPackages()
        {
            PowershellInstance.Commands.Clear();
            PowershellInstance.AddCommand("Get-AppxPackage");
            return PowershellInstance.Invoke();
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
                var il = props["InstallLocation"];
                installLocation = il.Value as string; // throws exception if value does not exist.
                if (string.IsNullOrEmpty(installLocation))
                {
                    installLocation = string.Empty;
                }
            }
            catch (GetValueInvocationException)
            {
                installLocation = string.Empty;
            }
            catch (Exception ex)
            {
                installLocation = string.Empty;
                Debug.WriteLine(ex);
            }

            return installLocation;
        }

        private static MachineType ResolveArchitecture(PSMemberInfoCollection<PSPropertyInfo> props) => props["Architecture"].Value.ToString() switch
        {
            "X64" => MachineType.X64,
            "X86" => MachineType.X86,
            _ => MachineType.Unknown,
        };

        private static string ResolveDisplayName(PSMemberInfoCollection<PSPropertyInfo> props, PSObject manifest)
        {
            string displayName;
            if (manifest == null)
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

            return displayName;
        }

        private static string ResolveExecutableName(PSObject manifest)
        {
            if (manifest == null)
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
            if (manifest == null)
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

            return installLocation.StartsWith(WindowsPath, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}