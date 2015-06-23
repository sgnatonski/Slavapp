using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.WinProcessWatcher
{
    internal class WMIWatcher : IWMIWatcher
    {
        private ManagementEventWatcher w = null;
        private Action<DateTime> notifyAction = t => { };
        private string processName;

        public WMIWatcher(string eventClassName, string processName, Action<DateTime> notifyAction)
        {
            this.notifyAction = notifyAction;
            this.processName = processName;
            var q = new WqlEventQuery();
            q.EventClassName = eventClassName;
            this.w = new ManagementEventWatcher(q);
            this.w.EventArrived += new EventArrivedEventHandler(ProcessStartEventArrived);
            this.w.Start();
        }

        private void ProcessStartEventArrived(object sender, EventArrivedEventArgs e)
        {
            var processName = e.NewEvent.Properties["ProcessName"].Value.ToString();
            if (processName.ToLowerInvariant() == this.processName.ToLowerInvariant())
            {
                //var timeCreated = (ulong)e.NewEvent.Properties["TIME_CREATED"].Value;
                this.notifyAction(DateTime.Now);
            }
        }

        public void Dispose()
        {
            if (this.w != null)
            {
                this.w.Stop();
            }
        }
    }
}
