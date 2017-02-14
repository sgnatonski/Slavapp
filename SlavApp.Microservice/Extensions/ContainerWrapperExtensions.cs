using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using SlavApp.Microservice.Microservice;
using SlavApp.Microservice.Network;
using SlavApp.Microservice.Requests;
using SlavApp.Microservice.Serialization;
using SlavApp.Microservice.Dispatching;
using SlavApp.Microservice.Log;

namespace SlavApp.Microservice.Extensions
{
    public static class ContainerWrapperExtensions
    {
        public static IContainerWrapper UseLogger<T>(this IContainerWrapper cw) where T : class, ILogger
        {
            cw.RegisterSingle<ILogger, T>();
            return cw;
        }

        public static IContainerWrapper UseNotifier<T>(this IContainerWrapper cw) where T : class, IMicroserviceStatusNotifier
        {
            cw.RegisterSingle<IMicroserviceStatusNotifier, T>();
            return cw;
        }

        public static void ConfigureServer(this IContainerWrapper cw, ServerOptions options)
        {
            cw.Register(NetMQContext.Create());
            cw.Register(options.ServerRequestConfiguration ?? new ServerRequestConfiguration { RequestTimeout = TimeSpan.FromMilliseconds(1000), RequestRetryLimit = 3 });
            cw.RegisterSingle<ServerRequestManager>();
            cw.RegisterSingle<XPubXSubProxy>();
            cw.RegisterSingle<RequestDispatcher>();
            cw.RegisterSingle<RequestPublisher>();
            cw.RegisterSingle<IpAddressService>();
            cw.Register(cw.Resolve<XPubXSubProxy>().GetEndpointConfiguration());
            ProtobufConfig.Initialize();
        }

        public static MicroserviceRunner ConfigureService(this IContainerWrapper cw, ServiceOptions options)
        {
            cw.Register(NetMQContext.Create());
            cw.Register(options.RequestEndpointConfiguration);
            cw.Register(new RequestHandlerFactory(cw));
            cw.Register(new MicroserviceFactory(cw));
            cw.Register<MicroserviceRunner>();
            cw.Register<RequestReceiver>();
            cw.RegisterSingle<HandlerFeeder>();
            ProtobufConfig.Initialize();

            var runner = cw.Resolve<MicroserviceRunner>();
            runner.Initialize(options);
            return runner;
        }
    }
}
