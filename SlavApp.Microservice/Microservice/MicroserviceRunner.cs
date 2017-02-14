using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Microservice
{
    public class MicroserviceRunner
    {
        private readonly List<IMicroservice> _services = new List<IMicroservice>();
        private readonly IMicroserviceStatusNotifier _msn;
        private readonly MicroserviceFactory _factory;

        public MicroserviceRunner(IMicroserviceStatusNotifier msn, MicroserviceFactory factory)
        {
            _msn = msn;
            _factory = factory;
        }

        internal void Initialize(ServiceOptions serviceOptions)
        {
            _msn.Initialize(serviceOptions.StatusQueueConnectionString, serviceOptions.StatusQueueName);
            AppDomain.CurrentDomain.ProcessExit += (se, ea) => _services.ForEach(s => s.Stop(_msn));
            AppDomain.CurrentDomain.UnhandledException += (se, ea) => _services.ForEach(s =>
            {
                _msn.Notify(s, MicroserviceStatus.Crashed);
                if (s.IsRunning())
                {
                    s.Stop(_msn);
                }
            });
        }

        public MicroserviceRunner RunService<T>() where T : class, IMicroservice
        {
            if (_services.Any(x => x.GetType() == typeof (T)))
            {
                throw new Exception(string.Format("Cannot run service of type {0} more than once.", typeof(T).FullName));
            }

            var service = _factory.Create<T>();
            _services.Add(service);
            service.Initialize(_msn);
            service.Start(_msn);

            return this;
        }
    }
}
