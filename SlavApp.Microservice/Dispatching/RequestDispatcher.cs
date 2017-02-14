using ConcurrentPriorityQueue;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Dispatching
{
    public class RequestDispatcher
    {
        private readonly ConcurrentPriorityQueue<Worker, double> workerQueue = new ConcurrentPriorityQueue<Worker, double>();
        public RequestDispatcher()
        {
            
        }

        public void SignalWorkerAvailable(int workerId)
        {
            workerQueue.Enqueue(new Worker(workerId, 100), 0);
        }

        public void SignalWorkDone(Worker worker, TimeSpan completedIn)
        {
            worker.AddTimeout(completedIn);
            workerQueue.UpdatePriority(worker, -worker.AverageTimeoutMilliseconds);
        }
        public void SignalTimeout(Worker worker, TimeSpan timeout)
        {
            worker.AddTimeout(timeout);
            workerQueue.UpdatePriority(worker, -worker.AverageTimeoutMilliseconds);
        }

        public Worker FirstAvailableWorker
        {
            get { return workerQueue.Peek(); }
        }
    }
}
