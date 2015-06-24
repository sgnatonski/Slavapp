using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications
{
    public class NotificationService
    {
        private readonly NotificationFactory factory;

        public NotificationService(NotificationFactory factory)
        {
            this.factory = factory;
        }

        public void Notify<TData>(TData data)
        {
            var notification = factory.Create<INotification<TData>>(typeof(TData));
            notification.Notify(data);
        }
    }
}
