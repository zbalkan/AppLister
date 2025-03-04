﻿using Engine.Junk.Finders;

namespace Engine.Junk.Containers
{
    internal class RegistryValueJunk : RegistryKeyJunk
    {
        /// <summary>
        ///     If not null, overrides ValueName in ResolveDisplayName
        /// </summary>
        public string DisplayValueName { get; set; }

        public string ValueName { get; }

        public RegistryValueJunk(string containingKeyPath, string valueName, ApplicationUninstallerEntry application,
                            IJunkCreator source) : base(containingKeyPath, application, source)
        {
            ValueName = valueName;
        }

        public override string GetDisplayName() => base.GetDisplayName() + " => " + (DisplayValueName ?? ValueName);
    }
}