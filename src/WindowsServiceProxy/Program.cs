using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;

namespace WindowsServiceProxy
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            EnsureSingleInstance();
            var service = new WindowsBackgroundService
            {
                ServiceName = "InventoryService"
            };
            InitiateEventLog(service);

            if (Environment.UserInteractive)
            {
                service.TestStartupAndStop(args);
            }
            else
            {
                ServiceBase.Run(new ServiceBase[]
                {
                service
                });
            }
        }

        private static void InitiateEventLog(WindowsBackgroundService service)
        {
            service.EventLog.Source = service.ServiceName;
            service.EventLog.Log = "Application";
            service.EventLog.BeginInit();
            if (!EventLog.SourceExists(service.EventLog.Source))
            {
                EventLog.CreateEventSource(service.EventLog.Source, service.EventLog.Log);
            }
            service.EventLog.EndInit();
        }

        private static void EnsureSingleInstance()
        {
            var proc = Process.GetCurrentProcess();
            var count = Process.GetProcesses().Count(p => p.ProcessName == proc.ProcessName);

            if (count > 1)
            {
                Environment.Exit(1);
            }
        }
    }
}