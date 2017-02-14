using System;
using SlavApp.Microservice.Network;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Requests
{
    public interface IRequestPublisher
    {
        ContextMessage PublishAndReceive<T, K>(ServerTopicHeader topicHeader, T message, TimeSpan timeout)
            where T : ContextMessage
            where K : ContextMessage;
    }
}