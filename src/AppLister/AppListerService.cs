using System;
using System.Diagnostics;

namespace AppLister
{
    public sealed class AppListerService : IDisposable
    {
        private readonly Discovery _discovery;

        private readonly EventLog _logger;

        private readonly Stopwatch _stopwatch;

        private readonly WmiScanner _wmiScanner;

        private bool _disposedValue;

        public AppListerService(EventLog logger)
        {
            _logger = logger;
            _wmiScanner = new WmiScanner();
            _discovery = new Discovery();
            _stopwatch = new Stopwatch();
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Refresh()
        {
            Publisher.CheckDependency();
            var publishedApps = _wmiScanner.GetAll();

            _logger?.WriteEntry("Running discovery scan.", EventLogEntryType.Information, 10);
            _stopwatch.Reset();
            _stopwatch.Start();
            var discoveredApps = _discovery.GetAll();
            _stopwatch.Stop();

            if (publishedApps?.Count > 0)
            {
                Publisher.Unpublish(publishedApps);
            }
            Publisher.Publish(discoveredApps);
            var message = $"Discovery scan completed.\nElapsed time: {_stopwatch.Elapsed}\nDiscovered Apps: {discoveredApps.Count}";
            _logger?.WriteEntry(message, EventLogEntryType.Information, 11);
        }

        private void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                _logger.Dispose();
            }

            _disposedValue = true;
        }
    }
}