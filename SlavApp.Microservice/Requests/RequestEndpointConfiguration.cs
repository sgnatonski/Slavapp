using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Requests
{
    public class RequestEndpointConfiguration
    {
        public string EndpointAddress { get; set; }
        public int RequestSubscribePort { get; set; }
        internal int RequestPublishPort { get; set; }
    }
}
