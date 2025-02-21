using System.IO;
using InventoryEngine.Junk.Finders;

namespace InventoryEngine.Junk.Containers
{
    internal class FileSystemJunk : JunkResultBase
    {
        public FileSystemInfo Path { get; }

        public FileSystemJunk(FileSystemInfo path, ApplicationUninstallerEntry application, IJunkCreator source) : base(application, source)
        {
            Path = path;
        }

        public override string GetDisplayName() => Path.FullName;
    }
}