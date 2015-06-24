using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications
{
    public interface INotificationReceiver
    {
        void Send<TData>(TData data);
    }
}
