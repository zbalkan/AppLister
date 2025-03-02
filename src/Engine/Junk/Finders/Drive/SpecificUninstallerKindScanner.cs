using System;
using System.Collections.Generic;
using System.IO;
using Engine.Junk.Confidence;
using Engine.Junk.Containers;

namespace Engine.Junk.Finders.Drive
{
    internal class SpecificUninstallerKindScanner : JunkCreatorBase
    {
        public override string CategoryName => "Junk_Drive_GroupName";

        public override IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            if (!File.Exists(target.UninstallerFullFilename))
            {
                yield break;
            }

            FileSystemJunk result;

            switch (target.UninstallerKind)
            {
                case UninstallerType.InstallShield:
                    var dirPath = Path.GetDirectoryName(target.UninstallerFullFilename);

                    if (dirPath == null)
                    {
                        yield break;
                    }

                    var targetDir = new DirectoryInfo(dirPath);
                    result = new FileSystemJunk(targetDir, target, this);
                    break;

                case UninstallerType.InnoSetup:
                case UninstallerType.Nsis:
                    result = new FileSystemJunk(new FileInfo(target.UninstallerFullFilename), target, this);
                    break;

                case UninstallerType.Msiexec:
                    if (target.UninstallerFullFilename.EndsWith("msiexec.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        yield break;
                    }

                    var path = new FileInfo(target.UninstallerFullFilename);
                    if ((path.Attributes & FileAttributes.System) == FileAttributes.System)
                    {
                        yield break;
                    }

                    result = new FileSystemJunk(path, target, this);
                    break;

                default:
                    yield break;
            }

            result.Confidence.Add(ConfidenceRecords.ExplicitConnection);

            yield return result;
        }
    }
}