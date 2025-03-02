using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Engine.Shared
{
    internal class ThreadedWorkSpreader<TData, TState> where TState : class
    {
        internal Func<TData, string> DataNameGetter { get; }

        internal int MaxThreadsPerBucket { get; }

        internal Func<IList<TData>, TState> StateGenerator { get; }

        internal Action<TData, TState> WorkLogic { get; }

        private readonly List<WorkerData> _workers = new List<WorkerData>();

        internal ThreadedWorkSpreader(int maxThreadsPerBucket, Action<TData, TState> workLogic,
            Func<IList<TData>, TState> stateGenerator, Func<TData, string> dataNameGetter)
        {
            if (maxThreadsPerBucket <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxThreadsPerBucket), maxThreadsPerBucket, "Minimum value is 1");
            }

            MaxThreadsPerBucket = maxThreadsPerBucket;
            StateGenerator = stateGenerator ?? throw new ArgumentNullException(nameof(stateGenerator));
            WorkLogic = workLogic ?? throw new ArgumentNullException(nameof(workLogic));
            DataNameGetter = dataNameGetter ?? throw new ArgumentNullException(nameof(dataNameGetter));
        }

        internal IEnumerable<TState> Join()
        {
            foreach (var workerData in _workers)
            {
                try
                {
                    workerData.Worker.Join();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception in worker thread: " + ex);
                }
            }

            return _workers.Select(x => x.State);
        }

        internal void Start(IList<IList<TData>> dataBuckets)
        {
            if (dataBuckets == null)
            {
                throw new ArgumentNullException(nameof(dataBuckets));
            }

            foreach (var itemBucket in dataBuckets)
            {
                if (itemBucket.Count == 0)
                {
                    continue;
                }

                var threadCount = Math.Min(MaxThreadsPerBucket, (itemBucket.Count / 10) + 1);

                var threadWorkItemCount = (itemBucket.Count / threadCount) + 1;

                for (var i = 0; i < threadCount; i++)
                {
                    var firstUnique = i * threadWorkItemCount;
                    var workerItems = itemBucket.Skip(firstUnique).Take(threadWorkItemCount).ToList();

                    var worker = new Thread(WorkerThread)
                    {
                        Name = nameof(ThreadedWorkSpreader<TData, TState>) + "_worker",
                        IsBackground = false
                    };
                    var workerData = new WorkerData(workerItems, worker, StateGenerator(itemBucket));
                    _workers.Add(workerData);
                    worker.Start(workerData);
                }
            }
        }

        private void WorkerThread(object obj)
        {
            if (obj is WorkerData workerInterface)
            {
                foreach (var data in workerInterface.Input)
                {
                    WorkLogic.Invoke(data, workerInterface.State);
                }
            }
            else
            {
                throw new ArgumentException("obj is not WorkerData", nameof(obj));
            }
        }

        private sealed class WorkerData
        {
            internal List<TData> Input { get; }

            internal TState State { get; }

            internal Thread Worker { get; }

            internal WorkerData(List<TData> input, Thread worker, TState state)
            {
                Input = input;
                Worker = worker;
                State = state;
            }
        }
    }
}