﻿using InventoryEngine.Shared;
using UninstallTools.Junk.Finders;

namespace InventoryEngine.Junk.Containers
{
    internal class RunProcessJunk : JunkResultBase
    {
        public ProcessStartCommand ProcessToStart { get; }

        private readonly string _junkName;

        public RunProcessJunk(ApplicationUninstallerEntry application, IJunkCreator source, ProcessStartCommand processToStart, string junkName) : base(application, source)
        {
            _junkName = junkName;
            ProcessToStart = processToStart;
        }

        public override string GetDisplayName() => $"{_junkName} ({ProcessToStart})";
    }
}