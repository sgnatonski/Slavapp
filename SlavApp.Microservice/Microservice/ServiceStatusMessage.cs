using System;

namespace SlavApp.Microservice.Microservice
{
    public class ServiceStatusMessage
    {
        public string Identity { get; set; }
        public DateTimeOffset DateOn { get; set; }
        public MicroserviceStatus Status { get; set; }
    }
}
