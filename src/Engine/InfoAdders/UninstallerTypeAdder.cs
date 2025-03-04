using System;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using Engine.Extensions;
using Engine.Shared;
using Engine.Tools;

namespace Engine.InfoAdders
{
    internal class UninstallerTypeAdder : IMissingInfoAdder
    {
        public bool AlwaysRun { get; }

        public string[] CanProduceValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.UninstallerKind)
        };

        // Let other adders run first in case they add uninstall strings
        public InfoAdderPriority Priority { get; } = InfoAdderPriority.RunLast;

        public string[] RequiredValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.UninstallString),
            nameof(ApplicationUninstallerEntry.InstallLocation)
        };

        public bool RequiresAllValues { get; }

        private static readonly Regex InnoSetupFilenameRegex = new Regex(@"unins\d\d\d", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static UninstallerType GetUninstallerType(string uninstallString)
        {
            // Detect MSI / Windows installer based on the uninstallation string e.g.
            // "C:\ProgramData\Package
            // PersistentCache\{33d1fd90-4274-48a1-9bc1-97e33d9c2d6f}\vcredist_x86.exe" /uninstall
            if (ApplicationEntryTools.PathPointsToMsiExec(uninstallString) || uninstallString.ContainsAll(
                new[] { @"\Package PersistentCache\{", @"}\", ".exe" }, StringComparison.OrdinalIgnoreCase))
            {
                return UninstallerType.Msiexec;
            }

            // Detect Sdbinst
            if (uninstallString.Contains("sdbinst", StringComparison.OrdinalIgnoreCase)
                && uninstallString.Contains(".sdb", StringComparison.OrdinalIgnoreCase))
            {
                return UninstallerType.SdbInst;
            }

            if (uninstallString.Contains(@"InstallShield Installation Information\{", StringComparison.OrdinalIgnoreCase))
            {
                return UninstallerType.InstallShield;
            }

            if (uninstallString.Contains("powershell.exe", StringComparison.OrdinalIgnoreCase) ||
                uninstallString.Contains(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                return UninstallerType.PowerShell;
            }

            if (!ProcessStartCommand.TryParse(uninstallString, out var ps)
                || !Path.IsPathRooted(ps.FileName)
                || !File.Exists(ps.FileName))
            {
                return UninstallerType.Unknown;
            }

            try
            {
                var fileName = Path.GetFileNameWithoutExtension(ps.FileName);

                // Detect Inno Setup
                if (fileName != null && InnoSetupFilenameRegex.IsMatch(fileName))
                {
                    // Check if Inno Setup Uninstall Log exists
                    if (File.Exists(ps.FileName.Substring(0, ps.FileName.Length - 3) + "dat"))
                    {
                        return UninstallerType.InnoSetup;
                    }
                }

                // Detect NSIS Nullsoft.NSIS. Slow, but there's no other way than to scan the file
                using var reader = new StreamReader(ps.FileName, Encoding.ASCII);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("Nullsoft", StringComparison.Ordinal))
                    {
                        return UninstallerType.Nsis;
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
            return UninstallerType.Unknown;
        }

        public void AddMissingInformation(ApplicationUninstallerEntry target)
        {
            if (target.UninstallerKind != UninstallerType.Unknown)
            {
                return;
            }

            var uninstallString = target.UninstallString;

            if (!string.IsNullOrEmpty(uninstallString))
            {
                target.UninstallerKind = GetUninstallerType(uninstallString);
            }
            else if (!string.IsNullOrEmpty(target.InstallLocation))
            {
                // We don't have a valid uninstaller, so tell simple delete adder to do its job and
                // make our own
                target.UninstallerKind = UninstallerType.SimpleDelete;
            }
        }
    }
}