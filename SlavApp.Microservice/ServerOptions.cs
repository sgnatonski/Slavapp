using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using SlavApp.Microservice.Requests;

namespace SlavApp.Microservice
{
    public class ServerOptions
    {
        public ServerRequestConfiguration ServerRequestConfiguration { get; set; }
    }
}
