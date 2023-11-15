/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;

namespace WindowsService.Engine.Startup
{
    public sealed class TaskEntry : StartupEntryBase
    {
        internal TaskEntry(string name, string command, string commandFilename, Microsoft.Win32.TaskScheduler.Task task)
        {
            ProgramName = name;
            Command = command;
            CommandFilePath = Environment.ExpandEnvironmentVariables(commandFilename);
            SourceTask = task;

            ParentLongName = "Startup_ShortName_Task" + task.Path;
            EntryLongName = task.Name;

            FillInformationFromFile(CommandFilePath);
        }

        private Microsoft.Win32.TaskScheduler.Task SourceTask { get; }

        public override bool Disabled
        {
            get
            {
                try { return !SourceTask.Enabled; }
                catch (FileNotFoundException) { }
                catch (InvalidCastException) { }
                catch (System.Runtime.InteropServices.COMException) { }
                // If it's impossible to check disabled state, assume not disabled
                return false;
            }
            set
            {
                try
                {
                    SourceTask.Enabled = !value;
                }
                catch (Exception e)
                {
                    // TODO
                }
            }
        }

        public override string ParentShortName
        {
            get { return "Startup_ShortName_Task"; }
            protected set { }
        }

        public override void Delete()
        {
            SourceTask.Folder.DeleteTask(SourceTask.Name, false);
        }

        public override bool StillExists()
        {
            return TaskService.Instance.FindTask(SourceTask.Name) != null;
        }

        public override void CreateBackup(string backupPath)
        {
            File.WriteAllText(
                Path.Combine(backupPath, "Startup_ShortName_Task" + " - " + EntryLongName + ".xml"),
                SourceTask.Xml);
        }
    }
}