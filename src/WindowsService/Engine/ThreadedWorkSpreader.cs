/*
    Copyright (c) 2018 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace WindowsService.Engine
{
    internal class ThreadedWorkSpreader<TData, TState> where TState : class
    {
        private readonly List<WorkerData> _workers = new List<WorkerData>();

        public ThreadedWorkSpreader(int maxThreadsPerBucket, Action<TData, TState> workLogic,
            Func<IList<TData>, TState> stateGenerator, Func<TData, string> dataNameGetter)
        {
            if (maxThreadsPerBucket <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxThreadsPerBucket), maxThreadsPerBucket, @"Minimum value is 1");
            MaxThreadsPerBucket = maxThreadsPerBucket;
            StateGenerator = stateGenerator ?? throw new ArgumentNullException(nameof(stateGenerator));
            WorkLogic = workLogic ?? throw new ArgumentNullException(nameof(workLogic));
            DataNameGetter = dataNameGetter ?? throw new ArgumentNullException(nameof(dataNameGetter));
        }

        public Func<IList<TData>, TState> StateGenerator { get; }
        public Func<TData, string> DataNameGetter { get; }
        public Action<TData, TState> WorkLogic { get; }

        public int MaxThreadsPerBucket { get; }

        public void Start(IList<IList<TData>> dataBuckets)
        {
            if (dataBuckets == null) throw new ArgumentNullException(nameof(dataBuckets));

            var totalCount = dataBuckets.Aggregate(0, (i, list) => i + list.Count);

            foreach (var itemBucket in dataBuckets)
            {
                if (itemBucket.Count == 0) continue;

                var threadCount = Math.Min(MaxThreadsPerBucket, itemBucket.Count / 10 + 1);

                var threadWorkItemCount = itemBucket.Count / threadCount + 1;

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

        public IEnumerable<TState> Join()
        {
            foreach (var workerData in _workers)
                try
                {
                    workerData.Worker.Join();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Exception in worker thread: " + ex);
                }

            return _workers.Select(x => x.State);
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
                throw new ArgumentException(@"obj is not WorkerData", nameof(obj));
        }

        private sealed class WorkerData
        {
            public WorkerData(List<TData> input, Thread worker, TState state)
            {
                Input = input;
                Worker = worker;
                State = state;
            }

            public List<TData> Input { get; }
            public Thread Worker { get; }
            public TState State { get; }
        }
    }
}