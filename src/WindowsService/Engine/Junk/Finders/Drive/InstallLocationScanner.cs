/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System.Collections.Generic;
using WindowsService.Engine.Junk.Confidence;
using WindowsService.Engine.Junk.Containers;

namespace WindowsService.Engine.Junk.Finders.Drive
{
    public class InstallLocationScanner : JunkCreatorBase
    {
        public override IEnumerable<IJunkResult> FindJunk(ApplicationUninstallerEntry target)
        {
            if (!target.IsInstallLocationValid()) yield break;

            var resultNode = GetJunkNodeFromLocation(GetOtherInstallLocations(target), target.InstallLocation, target);
            if (resultNode != null)
            {
                if (target.UninstallerKind == UninstallerType.StoreApp)
                    resultNode.Confidence.Add(ConfidenceRecords.IsStoreApp);
                yield return resultNode;
            }
        }

        public override string CategoryName => "Junk_Drive_GroupName";
    }
}