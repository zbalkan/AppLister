using System;
using System.IO;
using Microsoft.Win32;
using InventoryEngine.Tools;

namespace InventoryEngine.Junk.Containers
{
    public class RegistryKeyJunk : JunkResultBase
    {
        public string FullRegKeyPath { get; }

        public RegistryKey OpenRegKey(bool writable = false)
        {
            return RegistryTools.OpenRegistryKey(FullRegKeyPath, writable);
        }

        public string RegKeyParentPath => Path.GetDirectoryName(FullRegKeyPath);
        public string RegKeyName => Path.GetFileName(FullRegKeyPath);

        public bool RegKeyExists()
        {
            using (var key = OpenRegKey())
                return key != null;
        }

        public RegistryKeyJunk(string fullRegKeyPath, ApplicationUninstallerEntry application, IJunkCreator source) : base(application, source)
        {
            if (string.IsNullOrEmpty(fullRegKeyPath))
                throw new ArgumentException(@"Argument is null or empty", nameof(fullRegKeyPath));

            FullRegKeyPath = fullRegKeyPath.TrimEnd('\\', '/', ' ');
        }

        public override void Backup(string backupDirectory)
        {
            var fileName = PathTools.SanitizeFileName(FullRegKeyPath.TrimStart('\\')) + ".reg";
            var path = Path.Combine(CreateBackupDirectory(backupDirectory), fileName);
            RegistryTools.ExportRegistry(path, new[] { FullRegKeyPath });
        }

        public override void Delete()
        {
            // removed
        }

        public override void Open()
        {
            if (!RegKeyExists())
                throw new IOException($"Key \"{FullRegKeyPath}\" doesn't exist or can't be accessed");

            RegistryTools.OpenRegKeyInRegedit(FullRegKeyPath);
        }

        public override string GetDisplayName()
        {
            return FullRegKeyPath;
        }
    }
}