using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications
{
    public class NotificationFactory : Dictionary<Type, Type>
    {
        private readonly IServiceProvider container;
 
        public NotificationFactory(IServiceProvider container, AssemblyProvider assemblyProvider)
        {
            this.container = container;
            var type = typeof(INotification<>);
            var types = (from x in assemblyProvider.GetTypes()
                         from z in x.GetInterfaces()
                         let y = x.BaseType
                         where
                         (y != null && y.IsGenericType && type.IsAssignableFrom(y.GetGenericTypeDefinition())) ||
                         (z.IsGenericType && type.IsAssignableFrom(z.GetGenericTypeDefinition()))
                         select x);

            foreach (var t in types)
            {
                var gi = t.GetInterfaces().FirstOrDefault(x => x.IsGenericType);
                if (gi != null)
                {
                    var gType = gi.GetGenericArguments()[0];
                    this.Add(gType, t);
                }
            }
        }

        public T Create<T>(Type type)
        {
            T val = (T)this.container.GetService(this[type]);
            return val;
        }
    }
}
