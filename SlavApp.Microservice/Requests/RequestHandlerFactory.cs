using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Requests
{
    public class RequestHandlerFactory
    {
        private readonly IContainerWrapper _cw;

        public RequestHandlerFactory(IContainerWrapper cw)
        {
            _cw = cw;
        }

        public T Create<T>() where T : class, IRequestHandler
        {
            return _cw.Resolve<T>();
        }
    }
}
