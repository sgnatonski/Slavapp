using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Requests
{
    public class ServerTopicHeader
    {
        public string Topic { get; set; }
        public int WorkerId { get; set; }
    }
}
