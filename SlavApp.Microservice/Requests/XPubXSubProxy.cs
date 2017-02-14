using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using SlavApp.Microservice.Network;

namespace SlavApp.Microservice.Requests
{
    public class XPubXSubProxy : IDisposable
    {
        private readonly NetMQContext _context;
        private readonly IpAddressService _ipService;
        private readonly XPublisherSocket _xpubSocket;
        private readonly XSubscriberSocket _xsubSocket;
        private readonly Poller _poller;
        private readonly Proxy _proxy;
        private RequestEndpointConfiguration requestEndpointConfig;

        public XPubXSubProxy(NetMQContext context, IpAddressService ipService)
        {
            _context = context;
            _ipService = ipService;

            _xpubSocket = _context.CreateXPublisherSocket();
            _xsubSocket = _context.CreateXSubscriberSocket();
            _poller = new Poller(_xsubSocket, _xpubSocket);
            _proxy = new Proxy(_xsubSocket, _xpubSocket, null, _poller);
        }

        public RequestEndpointConfiguration GetEndpointConfiguration()
        {
            if (requestEndpointConfig == null)
            {
                this.StartProxy();
            }
            return requestEndpointConfig;
        }

        public void StartProxy()
        {
            requestEndpointConfig = new RequestEndpointConfiguration();
            requestEndpointConfig.EndpointAddress = _ipService.GetLocalAddress();
            requestEndpointConfig.RequestSubscribePort = _xpubSocket.BindRandomPort("tcp://" + requestEndpointConfig.EndpointAddress);
            requestEndpointConfig.RequestPublishPort = _xsubSocket.BindRandomPort("tcp://" + requestEndpointConfig.EndpointAddress);

            _proxy.Start();
            _poller.PollTillCancelledNonBlocking();
        }

        public void Dispose()
        {
            if (_poller != null)
            {
                _poller.Dispose();
            }
            if (_xsubSocket != null)
            {
                _xsubSocket.Dispose();
            }
            if (_xpubSocket != null)
            {
                _xpubSocket.Dispose();
            }
        }
    }
}
