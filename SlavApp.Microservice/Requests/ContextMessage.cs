using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Requests
{
    public class ContextMessage
    {
        public string RawData { get; set; }
        public string User { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
