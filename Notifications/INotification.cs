using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications
{
    public interface INotification<TData>
    {
        void Notify(TData data);
    }
}
