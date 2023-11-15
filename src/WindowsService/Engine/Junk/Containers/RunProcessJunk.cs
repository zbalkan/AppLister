/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System;
using System.Diagnostics;

namespace WindowsService.Engine.Junk.Containers
{
    public class RunProcessJunk : JunkResultBase
    {
        public ProcessStartCommand ProcessToStart { get; }

        private readonly string _junkName;

        public RunProcessJunk(ApplicationUninstallerEntry application, IJunkCreator source, ProcessStartCommand processToStart, string junkName) : base(application, source)
        {
            _junkName = junkName;
            ProcessToStart = processToStart;
        }

        public override void Backup(string backupDirectory)
        {

        }

        public override void Delete()
        {
            // removed
        }

        public override string GetDisplayName()
        {
            return $"{_junkName} ({ProcessToStart})";
        }

        public override void Open()
        {
            // removed
        }
    }
}