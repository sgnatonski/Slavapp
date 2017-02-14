using SlavApp.Microservice.Network;
using System.Collections.Generic;

namespace SlavApp.Microservice.Requests
{
    public interface IRequestReceiver
    {
        IEnumerable<T> Receive<T>(IHandleTopic topic) where T : ResponseContextMessage;
        void Reply(int replyPort, ContextMessage replyMessage);
    }
}