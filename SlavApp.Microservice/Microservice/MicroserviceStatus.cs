using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Microservice
{
    public enum MicroserviceStatus
    {
        Undefined,
        StartingUp,
        Running,
        NotResponding,
        Crashed,
        Stopped
    }
}
