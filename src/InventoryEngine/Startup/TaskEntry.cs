using System;
using Microsoft.Win32.TaskScheduler;

namespace InventoryEngine.Startup
{
    internal sealed class TaskEntry : StartupEntryBase, IDisposable
    {
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override bool StillExists() => TaskService.Instance.FindTask(SourceTask.Name) != null;

        private void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                SourceTask.Dispose();
            }
            _disposedValue = true;
        }
    }
}