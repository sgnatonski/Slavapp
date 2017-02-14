using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Microservice
{
    public interface IMicroserviceStatusNotifier
    {
        void Initialize(string queueConnString, string queueName);
        void Notify(IMicroservice service, MicroserviceStatus status);
    }
}
