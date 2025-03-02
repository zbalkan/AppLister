using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace WindowsServiceProxy
{
    public partial class WindowsBackgroundService : ServiceBase
    {
        private const int DefaultQueryPeriodInMinutes = 600;

        // Default period constant
        private const string QueryPeriodKey = "QueryPeriodInMinutes";

        private const string ServiceKeyPath = @"SOFTWARE\zb\AppListerSvc";

        private readonly AppLister.AppLister _internalService;

        private readonly int _queryPeriodInMilliseconds;

        private Timer _timer;

        public WindowsBackgroundService()
        {
            InitializeComponent();
            using (var key = Registry.LocalMachine.OpenSubKey(ServiceKeyPath))
            {
                if (key == null)
                {
                    throw new TaskCanceledException("Registry keys not found.");
                }

                var period = (int)key.GetValue(QueryPeriodKey, DefaultQueryPeriodInMinutes);
                _queryPeriodInMilliseconds = ToMillisecond(period);
            }

            _internalService = new AppLister.AppLister(EventLog);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("AsyncUsage", "AsyncFixer01:Unnecessary async/await usage", Justification = "<Pending>")]
        internal async Task TestStartupAndStop(string[] args)
        {
            OnStart(args);
            await Task.Delay(Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            EventLog.WriteEntry("Service started.", EventLogEntryType.Information, 1);

            try
            {
                _timer = new Timer(Refresh, null, 0, _queryPeriodInMilliseconds);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                Stop();
            }
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Service stopped.", EventLogEntryType.Information, 2);

            _timer.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Data types", "TI0301:Do not use 'magic numbers'", Justification = "Converting minutes to milliseconds")]
        private static int ToMillisecond(int period) => period * 60 * 1000;

        private void Refresh(object state)
        {
            try
            {
                _internalService.Refresh();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error, 3);
            }
        }
    }
}