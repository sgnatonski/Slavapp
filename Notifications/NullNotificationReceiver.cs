using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications
{
    public class NullNotificationReceiver : INotificationReceiver
    {
        public void Send<TData>(TData data)
        {
            // do nothing
        }
    }
}
