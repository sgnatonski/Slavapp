using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using SlavApp.Microservice.Requests;

namespace SlavApp.Microservice
{
    public class ServiceOptions
    {
        public RequestEndpointConfiguration RequestEndpointConfiguration { get; set; }
        public string StatusQueueConnectionString { get; set; }
        public string StatusQueueName { get; set; }
    }
}
