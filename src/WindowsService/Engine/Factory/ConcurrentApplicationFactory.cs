using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace WindowsService.Engine.Factory
{
    internal sealed class ConcurrentApplicationFactory : IDisposable
    {
        private bool _cancelled;

        private readonly Thread _thread;

        private List<ApplicationUninstallerEntry> _threadResults;

        public ConcurrentApplicationFactory(
            Func<List<ApplicationUninstallerEntry>> factoryMethod)
        {
            _thread = new Thread(() =>
            {
                try
                {
                    _threadResults = factoryMethod();
                }
                catch (OperationCanceledException)
                {
                    _cancelled = true;
                }
            })
            {
                IsBackground = false,
                Name = "ConcurrentGetUninstallerEntries",
                Priority = ThreadPriority.Normal
            };
        }

        public void Dispose()
        {
            _cancelled = true;
        }

        public void Start()
        {
            _thread.Start();
        }

        public List<ApplicationUninstallerEntry> GetResults()
        {
            Debug.Assert(_thread != null);

            if (_thread.IsAlive)
            {
                _thread.Join();
            }

            if (_cancelled)
                throw new OperationCanceledException();

            return _threadResults ?? new List<ApplicationUninstallerEntry>();
        }
    }
}