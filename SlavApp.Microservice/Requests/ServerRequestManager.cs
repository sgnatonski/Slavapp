using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using SlavApp.Microservice.Network;
using NetMQ.Sockets;
using SlavApp.Microservice.Dispatching;

namespace SlavApp.Microservice.Requests
{
    public class ServerRequestManager
    {
        private readonly RequestPublisher _requestPublisher;
        private readonly RequestDispatcher _dispatcher;
        private readonly ServerRequestConfiguration _config;

        public ServerRequestManager(RequestPublisher requestPublisher, RequestDispatcher dispatcher, ServerRequestConfiguration config)
        {
            _requestPublisher = requestPublisher;
            _dispatcher = dispatcher;
            _config = config;
        }

        public object Handle(string path, string method, string body)
        {
            var topic = string.Format("{0}:{1}", method, path);
            var sendMsg = new ContextMessage()
            {
                CreatedAt = DateTimeOffset.Now,
                User = Guid.NewGuid().ToString(),
                RawData = body
            };

            var worker = _dispatcher.FirstAvailableWorker;
            var topicHeader = new ServerTopicHeader
            {
                Topic = topic,
                WorkerId = worker.WorkerId
            };
            
            var retries = 0;
            ContextMessage message = null;
            do
            {
                if (retries > 0)
                {
                    Console.WriteLine(string.Format("{0} retry!", retries));
                    _dispatcher.SignalTimeout(worker, DateTimeOffset.Now - message.CreatedAt);
                }
                message = _requestPublisher.PublishAndReceive<ContextMessage, ContextMessage>(topicHeader, sendMsg, _config.RequestTimeout);
                retries++;
            }
            while (CanContinueRetrying(retries, message));

            if (message is TimeoutContextMessage)
            {
                _dispatcher.SignalTimeout(worker, DateTimeOffset.Now - message.CreatedAt);
                Console.WriteLine(string.Format("{0} Timeout!", retries));
                return string.Format("{0} Timeout!", retries);
            }

            //Console.WriteLine("Received response {0} after {1} ms", message.User, (DateTimeOffset.Now - message.CreatedAt).Milliseconds);

            _dispatcher.SignalWorkDone(worker, DateTimeOffset.Now - message.CreatedAt);

            return message;
        }

        private bool CanContinueRetrying(int retries, ContextMessage message)
        {
            return retries < _config.RequestRetryLimit && (message == null || message is TimeoutContextMessage);
        }
    }
}
