using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Microservice
{
    public class MicroserviceFactory
    {
        private readonly IContainerWrapper _cw;

        public MicroserviceFactory(IContainerWrapper cw)
        {
            _cw = cw;
        }

        public T Create<T>() where T : class, IMicroservice
        {
            return _cw.Resolve<T>();
        }
    }
}
