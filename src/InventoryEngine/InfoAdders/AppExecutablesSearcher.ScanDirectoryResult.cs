using System.Collections.Generic;
using System.IO;

namespace InventoryEngine.InfoAdders
{
    internal partial class AppExecutablesSearcher
    {
        internal sealed class ScanDirectoryResult
        {
            public ICollection<DirectoryInfo> BinSubdirs { get; }

            public ICollection<FileInfo> ExecutableFiles { get; }

            public ICollection<DirectoryInfo> OtherSubdirs { get; }

            public ScanDirectoryResult(ICollection<FileInfo> executableFiles,
                                                    ICollection<DirectoryInfo> binSubdirs, ICollection<DirectoryInfo> otherSubdirs)
            {
                OtherSubdirs = otherSubdirs;
                ExecutableFiles = executableFiles;
                BinSubdirs = binSubdirs;
            }
        }
    }
}