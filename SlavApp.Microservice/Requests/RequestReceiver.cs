using NetMQ;
using SlavApp.Microservice.Extensions;
using SlavApp.Microservice.Log;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SlavApp.Microservice.Requests
{
    public class RequestReceiver : IRequestReceiver
    {
        private readonly NetMQContext _context;
        private readonly ILogger _logger;
        private readonly RequestEndpointConfiguration _requestEndpointConfig;

        public RequestReceiver(NetMQContext context, RequestEndpointConfiguration requestEndpointConfig, ILogger logger)
        {
            _context = context;
            _logger = logger;
            _requestEndpointConfig = requestEndpointConfig;
        }

        public IEnumerable<T> Receive<T>(IHandleTopic handle) where T : ResponseContextMessage
        {
            using (var subSocket = _context.CreateSubscriberSocket())
            {
                var endpoint = "tcp://" + _requestEndpointConfig.EndpointAddress + ":" + _requestEndpointConfig.RequestSubscribePort;
                var topicHeader = new ServerTopicHeader { Topic = handle.Topic, WorkerId = 1 };
                subSocket.Options.ReceiveHighWatermark = 1000;
                subSocket.Connect(endpoint);
                subSocket.Subscribe(topicHeader.Serialize());
                Console.WriteLine(string.Format("Listening for {0} on {1}", handle.Topic, endpoint));

                T response = null;
                while (true)
                {
                    try
                    {
                        bool more = false;
                        var messageTopicReceived = subSocket.ReceiveFrameBytes(out more);
                        if (more)
                        {
                            var messageReceived = subSocket.ReceiveFrameBytes();
                            //Console.WriteLine("Received " + messageTopicReceived);
                            response = messageReceived.Deserialize<T>();
                        }
                    }
                    catch (TerminatingException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }

                    if (response != null)
                    {
                        yield return response;
                    }
                }
            }
        }

        public void Reply(int replyPort, ContextMessage replyMessage)
        {
            var endpoint = "tcp://" + _requestEndpointConfig.EndpointAddress + ":" + replyPort;
            try
            {
                using (var replySocket = _context.CreateRequestSocket())
                {
                    //Console.WriteLine("Replying to " + endpoint);
                    replySocket.Connect(endpoint);
                    replySocket.SendFrame(replyMessage.Serialize());
                }
            }
            catch (TerminatingException)
            {
                // nothing to do
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}