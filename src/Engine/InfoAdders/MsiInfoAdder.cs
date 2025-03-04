﻿using System;
using Engine.Tools;

namespace Engine.InfoAdders
{
    internal class MsiInfoAdder : IMissingInfoAdder
    {
        public bool AlwaysRun { get; } = false;

        public string[] CanProduceValueNames { get; } =  {
            nameof(ApplicationUninstallerEntry.RawDisplayName),
            nameof(ApplicationUninstallerEntry.DisplayVersion),
            nameof(ApplicationUninstallerEntry.RawPublisher),
            nameof(ApplicationUninstallerEntry.InstallLocation),
            nameof(ApplicationUninstallerEntry.InstallSource),
            nameof(ApplicationUninstallerEntry.UninstallerFullFilename),
            nameof(ApplicationUninstallerEntry.AboutUrl),
            nameof(ApplicationUninstallerEntry.InstallDate)
        };

        public InfoAdderPriority Priority { get; } = InfoAdderPriority.RunFirst;

        public string[] RequiredValueNames { get; } = {
            nameof(ApplicationUninstallerEntry.UninstallerKind),
            nameof(ApplicationUninstallerEntry.BundleProviderKey)
        };

        public bool RequiresAllValues { get; } = true;

        public void AddMissingInformation(ApplicationUninstallerEntry target)
        {
            if (target.UninstallerKind != UninstallerType.Msiexec)
            {
                return;
            }

            ApplyMsiInfo(target, target.BundleProviderKey);
        }

        /// <summary>
        ///     A valid guid is REQUIRED. It doesn't have to be set on the entry, but should be.
        /// </summary>
        private static void ApplyMsiInfo(ApplicationUninstallerEntry entry, Guid guid)
        {
            //IMPORTANT: If MsiGetProductInfo returns null it means that the guid is invalid or app is not installed
            if (MsiTools.MsiGetProductInfo(guid, MsiWrapper.Installproperty.Productname) == null)
            {
                return;
            }

            FillInMissingInfoMsiHelper(() => entry.RawDisplayName, x => entry.RawDisplayName = x, guid,
                MsiWrapper.Installproperty.Installedproductname, MsiWrapper.Installproperty.Productname);

            FillInMissingInfoMsiHelper(() => entry.DisplayVersion, x => entry.DisplayVersion = ApplicationEntryTools.CleanupDisplayVersion(x), guid,
                MsiWrapper.Installproperty.Versionstring, MsiWrapper.Installproperty.Version);

            FillInMissingInfoMsiHelper(() => entry.RawPublisher, x => entry.RawPublisher = x, guid,
                MsiWrapper.Installproperty.Publisher);

            FillInMissingInfoMsiHelper(() => entry.InstallLocation, x => entry.InstallLocation = x, guid,
                MsiWrapper.Installproperty.Installlocation);

            FillInMissingInfoMsiHelper(() => entry.InstallSource, x => entry.InstallSource = x, guid,
                MsiWrapper.Installproperty.Installsource);

            FillInMissingInfoMsiHelper(() => entry.UninstallerFullFilename, x => entry.UninstallerFullFilename = x, guid,
                MsiWrapper.Installproperty.Localpackage);

            FillInMissingInfoMsiHelper(() => entry.AboutUrl, x => entry.AboutUrl = x, guid,
                MsiWrapper.Installproperty.Helplink, MsiWrapper.Installproperty.Urlupdateinfo,
                MsiWrapper.Installproperty.Urlinfoabout);

            if (entry.InstallDate != default)
            {
                return;
            }

            var temp = MsiTools.MsiGetProductInfo(guid, MsiWrapper.Installproperty.Installdate);
            if (string.IsNullOrEmpty(temp))
            {
                return;
            }

            try
            {
                entry.InstallDate = new DateTime(int.Parse(temp.Substring(0, 4)),
                    int.Parse(temp.Substring(4, 2)),
                    int.Parse(temp.Substring(6, 2)));
            }
            catch
            {
                // Date had invalid format, default to nothing
            }
        }

        private static void FillInMissingInfoMsiHelper(Func<string> get, Action<string> set, Guid guid,
            params MsiWrapper.Installproperty[] properties)
        {
            if (!string.IsNullOrEmpty(get()))
            {
                return;
            }

            foreach (var item in properties)
            {
                var temp = MsiTools.MsiGetProductInfo(guid, item);

                //IMPORTANT: Do not assign empty strings, they will mess up automatic property creation later on.
                if (string.IsNullOrEmpty(temp))
                {
                    set(temp);
                }
            }
        }
    }
}