using System;
using System.IO;
using InventoryEngine.Junk.Finders;

namespace InventoryEngine.Junk.Containers
{
    internal class RegistryKeyJunk : JunkResultBase
    {
        public string FullRegKeyPath { get; }

        public string RegKeyName => Path.GetFileName(FullRegKeyPath);

        public RegistryKeyJunk(string fullRegKeyPath, ApplicationUninstallerEntry application, IJunkCreator source) : base(application, source)
        {
            if (string.IsNullOrEmpty(fullRegKeyPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(fullRegKeyPath));
            }

            FullRegKeyPath = fullRegKeyPath.TrimEnd('\\', '/', ' ');
        }

        public override string GetDisplayName() => FullRegKeyPath;
    }
}