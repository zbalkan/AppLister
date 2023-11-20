using System;
using System.Diagnostics;
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
    }
}