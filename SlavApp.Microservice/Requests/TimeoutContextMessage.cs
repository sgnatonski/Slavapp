using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Requests
{
    public class TimeoutContextMessage : ContextMessage
    {
        public TimeoutContextMessage(ContextMessage message, TimeSpan timeout)
        {
            Message = message;
            Timeout = timeout;
        }

        public ContextMessage Message { get; private set; }
        public TimeSpan Timeout { get; private set; }
    }
}
