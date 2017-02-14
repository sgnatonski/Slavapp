using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Requests
{
    public class ResponseContextMessage
    {
        public ResponseContextMessage(ContextMessage message, int responsePort)
        {
            Message = message;
            ResponsePort = responsePort;
        }

        public ContextMessage Message { get; private set; }
        public int ResponsePort { get; private set; }
    }
}
