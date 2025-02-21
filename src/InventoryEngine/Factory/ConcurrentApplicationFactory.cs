using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace InventoryEngine.Factory
{
    internal sealed class ConcurrentApplicationFactory : IDisposable
    {
        private readonly Thread _thread;

        private bool _cancelled;

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

        public void Dispose() => _cancelled = true;

        public List<ApplicationUninstallerEntry> GetResults()
        {
            Debug.Assert(_thread != null);

            if (_thread.IsAlive)
            {
                _thread.Join();
            }

            if (_cancelled)
            {
                throw new OperationCanceledException();
            }

            return _threadResults ?? new List<ApplicationUninstallerEntry>();
        }

        public void Start() => _thread.Start();
    }
}