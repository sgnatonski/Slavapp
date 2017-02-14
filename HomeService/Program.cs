using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SlavApp.Microservice;
using SlavApp.Microservice.Microservice;
using SlavApp.Microservice.Requests;
using SlavApp.Microservice.Log;
using SlavApp.Microservice.AzureNotifier;
using SlavApp.Microservice.Serialization;
using System.Net;
using SimpleInjector;
using SlavApp.Microservice.Extensions;
using SlavApp.Microservice.SimpleInjector;

namespace HomeService
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var cw = new SimpleInjectorContainerWrapper(new Container());
            cw.Register<HomeService>();

            var options = new ServiceOptions()
            {
                RequestEndpointConfiguration = GetRequestEndpointConfiguration(),
                StatusQueueConnectionString = "",
                StatusQueueName = ""
            };

            cw.UseLogger<TraceLogger>()
                .UseNotifier<AzureStatusNotifier>()
                .ConfigureService(options)
                .RunService<HomeService>();

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static RequestEndpointConfiguration GetRequestEndpointConfiguration()
        {
            RequestEndpointConfiguration config = null;
            do
            {
                try
                {
                    var url = "http://localhost:1234/endpoint";
                    Console.WriteLine("Waiting for server configuration on {0}", url);
                    var req = WebRequest.Create(url);
                    var resp = req.GetResponse();
                    var sr = new System.IO.StreamReader(resp.GetResponseStream());
                    var addr = sr.ReadToEnd().Trim().Split(':');
                    config = new RequestEndpointConfiguration()
                    {
                        EndpointAddress = addr[0],
                        RequestSubscribePort = int.Parse(addr[1])
                    };
                    Console.WriteLine("Configuration aqcuired from {0}", url);
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                }
            } while (config == null);
            return config;
        }
    }
}
