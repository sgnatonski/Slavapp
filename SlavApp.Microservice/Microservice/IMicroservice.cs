using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Microservice
{
    public interface IMicroservice
    {
        string Identifier { get; }
        void Initialize(IMicroserviceStatusNotifier notifier);
        void Start(IMicroserviceStatusNotifier notifier);
        void Stop(IMicroserviceStatusNotifier notifier);
        bool IsRunning();
    }
}
