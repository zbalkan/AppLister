using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace WindowsService
{
    public partial class WindowsBackgroundService : ServiceBase
    {
        private readonly InventoryService internalService;
        private readonly int _queryPeriodInMilliseconds;

        private const int DefaultQueryPeriodInMinutes = 600;  // Default period constant
        private const string QueryPeriodKey = "QueryPeriodInMinutes";
        private const string ServiceKeyPath = @"SOFTWARE\zb\InventoryService";

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
                _queryPeriodInMilliseconds = period * 60 * 1000;
            }

            internalService = new InventoryService(EventLog, ServiceKeyPath);
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

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

        protected override void OnStop() => _timer.Dispose();

        internal void TestStartupAndStop(string[] args)
        {
            OnStart(args);
            while (true) { }
            //OnStop();
        }

        private void Refresh(object state) => internalService.Refresh();
    }
}