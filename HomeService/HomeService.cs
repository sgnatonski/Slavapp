using System;
using System.Threading.Tasks;
using SlavApp.Microservice.Microservice;
using SlavApp.Microservice.Requests;
using HomeService.Handlers;

namespace HomeService
{
    public class HomeService : IMicroservice
    {
        private readonly RequestReceiver _receiver;
        private readonly HandlerFeeder _feeder;
        private readonly HomeHandler _handler;
        private bool _isRunning;

        public HomeService(RequestReceiver receiver, RequestHandlerFactory handlerFactory, HandlerFeeder feeder)
        {
            _receiver = receiver;
            _feeder = feeder;
            _handler = handlerFactory.Create<HomeHandler>();
        }

        public string Identifier
        {
            get { return string.Format("{0}@{1}", this.GetType().Name, Environment.MachineName); }
        }

        public void Initialize(IMicroserviceStatusNotifier notifier)
        {
            notifier.Notify(this, MicroserviceStatus.StartingUp);
        }

        public void Start(IMicroserviceStatusNotifier notifier)
        {
            Task.Run(() => _feeder.Feed(_handler, _receiver));
            this._isRunning = true;
            notifier.Notify(this, MicroserviceStatus.Running);
        }

        public void Stop(IMicroserviceStatusNotifier notifier)
        {
            this._isRunning = false;
            notifier.Notify(this, MicroserviceStatus.Stopped);
        }

        public bool IsRunning()
        {
            return this._isRunning;
        }
    }
}
