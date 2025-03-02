using System.Collections.Generic;
using Engine.Junk.Confidence;
using Engine.Junk.Containers;

namespace Engine.Junk.Finders.Drive
{
    internal class InstallLocationScanner : JunkCreatorBase
    {
        public override string CategoryName => "Junk_Drive_GroupName";

        public override IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            if (!target.IsInstallLocationValid())
            {
                yield break;
            }

            var resultNode = GetJunkNodeFromLocation(GetOtherInstallLocations(target), target.InstallLocation, target);
            if (resultNode == null)
            {
                yield break;
            }

            if (target.UninstallerKind == UninstallerType.StoreApp)
            {
                resultNode.Confidence.Add(ConfidenceRecords.IsStoreApp);
            }

            yield return resultNode;
        }
    }
}