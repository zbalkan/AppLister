using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;

namespace WindowsService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
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
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                service
                };

                ServiceBase.Run(ServicesToRun);
            }
        }

        private static void InitiateEventLog(WindowsBackgroundService service)
        {
            service.EventLog.Source = service.ServiceName;
            service.EventLog.Log = "Application";
            ((ISupportInitialize)service.EventLog).BeginInit();
            if (!EventLog.SourceExists(service.EventLog.Source))
            {
                EventLog.CreateEventSource(service.EventLog.Source, service.EventLog.Log);
            }
                    ((ISupportInitialize)service.EventLog).EndInit();
        }
    }
}
