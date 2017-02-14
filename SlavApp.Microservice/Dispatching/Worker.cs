using System;
using System.Linq;
using System.Collections.Generic;
using ConcurrentPriorityQueue;

namespace SlavApp.Microservice.Dispatching
{
    public class Worker
    {
        private ConcurrentPriorityQueue<TimeSpan, bool> timeouts;
        private int timeoutsCount = 100;
        private double _averageTimeoutMilliseconds;
        public Worker(int workerId, int maxTimeouts)
        {
            WorkerId = workerId;
            timeouts = new ConcurrentPriorityQueue<TimeSpan, bool>();
            timeoutsCount = maxTimeouts;
        }

        public int WorkerId { get; private set; }

        internal void AddTimeout(TimeSpan timeout)
        {
            if (timeouts.Count > timeoutsCount)
            {
                timeouts.Dequeue();
            }
            timeouts.Enqueue(timeout, true);
            _averageTimeoutMilliseconds = timeouts.Average(x => x.Milliseconds);
        }

        public double AverageTimeoutMilliseconds { get { return _averageTimeoutMilliseconds; } }
    }
}