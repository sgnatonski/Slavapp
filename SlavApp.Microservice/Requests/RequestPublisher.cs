using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using SlavApp.Microservice.Extensions;
using SlavApp.Microservice.Network;
using System.Threading;
using NetMQ.Sockets;
using System.Net.Sockets;
using SlavApp.Microservice.Log;
using SlavApp.Microservice.Dispatching;

namespace SlavApp.Microservice.Requests
{
    public class RequestPublisher : IRequestPublisher, IDisposable
    {
        private readonly NetMQContext _context;
        private readonly ILogger _logger;
        private readonly PublisherSocket _requestSocket;
        
        public RequestPublisher(NetMQContext context, RequestEndpointConfiguration requestEndpointConfig, ILogger logger)
        {
            _context = context;
            _logger = logger;

            _requestSocket = _context.CreatePublisherSocket();
            var endpoint = "tcp://" + requestEndpointConfig.EndpointAddress + ":" + requestEndpointConfig.RequestPublishPort;
            _requestSocket.Options.SendHighWatermark = 1000;
            _requestSocket.Connect(endpoint);
        }

        public ContextMessage PublishAndReceive<T, K>(ServerTopicHeader topicHeader, T message, TimeSpan timeout)
            where T : ContextMessage
            where K : ContextMessage
        {
            try
            {
                using (var responseSocket = _context.CreateResponseSocket())
                {
                    var responsePort = responseSocket.BindRandomPort("tcp://*");

                    var responseEnvelope = new ResponseContextMessage(message, responsePort);
                    
                    _requestSocket
                        .SendMoreFrame(topicHeader.Serialize())
                        .SendFrame(responseEnvelope.Serialize());

                    byte[] buffer = null;
                    if (responseSocket.TryReceiveFrameBytes(timeout, out buffer))
                    {
                        return buffer.Deserialize<K>();
                    }

                    // timeout limit reached, report timeout
                    return new TimeoutContextMessage(message, timeout);
                }
            }
            catch (TerminatingException)
            {
                return new TimeoutContextMessage(message, timeout);
            }
            catch (SocketException se)
            {
                // socket error, report timeout, so that request can be replayed
                _logger.Error(se);
                return new TimeoutContextMessage(message, timeout);
            }
            catch (NetMQException ne)
            {
                _logger.Error(ne);
                throw;
            }
        }

        public void Dispose()
        {
            if (_requestSocket != null)
            {
                _requestSocket.Dispose();
            }
        }
    }
}
