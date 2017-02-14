using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Requests
{
    public class HandlerFeeder
    {
        public void Feed(IRequestHandler handler, IRequestReceiver receiver)
        {
            foreach (var message in receiver.Receive<ResponseContextMessage>(handler))
            {
                Task.Run(() =>
                {
                    var response = handler.Handle(message);
                    receiver.Reply(message.ResponsePort, response);
                });
            }
        }
    }
}
