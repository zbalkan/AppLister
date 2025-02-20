using System;
using System.IO;
using Microsoft.Win32.TaskScheduler;

namespace InventoryEngine.Startup
{
    internal sealed class TaskEntry : StartupEntryBase, IDisposable
    {
        public bool Disabled
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
                catch (Exception)
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

        private Task SourceTask { get; }
        private bool _disposedValue;

        internal TaskEntry(string name, string command, string commandFilename, Task task)
        {
            ProgramName = name;
            Command = command;
            CommandFilePath = Environment.ExpandEnvironmentVariables(commandFilename);
            SourceTask = task;

            ParentLongName = "Startup_ShortName_Task" + task.Path;
            EntryLongName = task.Name;

            FillInformationFromFile(CommandFilePath);
        }

        public void Delete() => SourceTask.Folder.DeleteTask(SourceTask.Name, false);

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override bool StillExists() => TaskService.Instance.FindTask(SourceTask.Name) != null;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    SourceTask.Dispose();
                }
                _disposedValue = true;
            }
        }
    }
}