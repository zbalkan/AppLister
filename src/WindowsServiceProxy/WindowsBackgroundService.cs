using System;
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

        private const string ServiceKeyPath = @"SOFTWARE\zb\InventorySvc";

        private readonly InventoryService.InventoryService _internalService;

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

            _internalService = new InventoryService.InventoryService(EventLog);
        }

        internal void TestStartupAndStop(string[] args)
        {
            OnStart(args);
            while (true)
            {
                // empty loop to keep the main thread alive
            }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Data types", "TI0301:Do not use 'magic numbers'", Justification = "Converting minutes to milliseconds")]
        private static int ToMillisecond(int period) => period * 60 * 1000;

        private void Refresh(object state) => _internalService.Refresh();
    }
}