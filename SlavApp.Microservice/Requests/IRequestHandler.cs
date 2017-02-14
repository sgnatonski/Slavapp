using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Requests
{
    public interface IRequestHandler : IHandleTopic
    {
        ContextMessage Handle<T>(T message) where T : ResponseContextMessage;
    }
}
