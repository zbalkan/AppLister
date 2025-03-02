using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace WindowsServiceProxy
{
    internal static class Program
    {
        private static void EnsureSingleInstance()
        {
            var proc = Process.GetCurrentProcess();
            var count = Process.GetProcesses().Count(p => p.ProcessName == proc.ProcessName);

            if (count > 1)
            {
                Environment.Exit(1);
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

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static async Task Main(string[] args)
        {
            EnsureSingleInstance();
            var service = new WindowsBackgroundService
            {
                ServiceName = "AppLister"
            };
            InitiateEventLog(service);

            if (Environment.UserInteractive)
            {
                await service.TestStartupAndStop(args);
            }
            else
            {
                ServiceBase.Run(new ServiceBase[]
                {
                    service
                });
            }
        }
    }
}