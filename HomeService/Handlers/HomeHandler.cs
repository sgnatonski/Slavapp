using System;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using SlavApp.Microservice.Requests;

namespace HomeService.Handlers
{
    public class HomeHandler : IRequestHandler
    {
        public string Topic { get { return "POST:/"; } }

        public ContextMessage Handle<T>(T message) where T: ResponseContextMessage
        {
            return new ContextMessage()
            {
                CreatedAt = DateTimeOffset.Now,
                User = message.Message.User
            };
        }
    }
}
