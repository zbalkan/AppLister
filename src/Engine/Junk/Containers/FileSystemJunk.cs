using System.IO;
using Engine.Junk.Finders;

namespace Engine.Junk.Containers
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