﻿/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System.IO;
using System.Linq;
using WindowsService.Engine.Factory;
using WindowsService.Engine.Startup;
using WindowsService.Engine.Tools;

namespace WindowsService.Engine
{
    public sealed class BrowserHelperEntry : StartupEntryBase
    {
        private bool _disabled;

        public BrowserHelperEntry(string programName, string command, string parentKeyPath,
            string className, bool disabled, bool isWow)
        {
            ProgramName = programName;
            Command = command;

            ParentLongName = parentKeyPath;
            EntryLongName = className;
            _disabled = disabled;
            IsWow = isWow;
            
            CommandFilePath = ProcessCommandString(Command);

            if(CommandFilePath != null)
                FillInformationFromFile(CommandFilePath);
        }

        private bool IsWow { get; }

        public override bool Disabled
        {
            get { return _disabled; }
            set
            {
                if (_disabled == value) return;

                using (var key = RegistryTools.OpenRegistryKey(GetRealParentPath(), true))
                using (var newKey = RegistryTools.CreateSubKeyRecursively(GetRealParentPath(true)))
                    key.MoveSubKey(EntryLongName, newKey, EntryLongName);

                _disabled = value;
            }
        }

        public override string ParentShortName
        {
            get { return "Startup_Shortname_BrowserHelper" + (IsWow ? " (Wow)" : string.Empty); }
            protected set { }
        }

        public string GetRealParentPath(bool opposite = false)
        {
            return (opposite ? !_disabled : _disabled)
                ? Path.Combine(ParentLongName, BrowserEntryFactory.AutorunsDisabledKeyName)
                : ParentLongName;
        }

        public override void Delete()
        {
            using (var key = RegistryTools.OpenRegistryKey(GetRealParentPath(), true))
                key?.DeleteSubKey(EntryLongName);
        }

        public override bool StillExists()
        {
            using (var key = RegistryTools.OpenRegistryKey(GetRealParentPath()))
                return key != null && key.GetSubKeyNames().Contains(EntryLongName);
        }

        public override void CreateBackup(string backupPath)
        {
            RegistryTools.ExportRegistry(Path.Combine(backupPath,
                $@"Startup_Shortname_BrowserHelper - {ProgramName}.reg"),
                Path.Combine(GetRealParentPath(), EntryLongName));
        }
    }
}