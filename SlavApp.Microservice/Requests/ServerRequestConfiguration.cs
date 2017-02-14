using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlavApp.Microservice.Requests
{
    public class ServerRequestConfiguration
    {
        public TimeSpan RequestTimeout { get; set; }

        public int RequestRetryLimit { get; set; }
    }
}
