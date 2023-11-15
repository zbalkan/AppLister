/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System.IO;

namespace WindowsService.Engine.Junk.Containers
{
    public class FileSystemJunk : JunkResultBase
    {
        public FileSystemJunk(FileSystemInfo path, ApplicationUninstallerEntry application, IJunkCreator source) : base(application, source)
        {
            Path = path;
        }

        public FileSystemInfo Path { get; }

        public override void Backup(string backupDirectory)
        {
            // Items are deleted to the recycle bin
        }

        public override string GetDisplayName()
        {
            return Path.FullName;
        }

        public override void Delete()
        {
            // removed
        }

        public override void Open()
        {
            // removed
        }
    }
}