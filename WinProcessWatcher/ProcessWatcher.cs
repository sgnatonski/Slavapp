using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.WinProcessWatcher
{
    public class ProcessWatcher
    {
        public IWMIWatcher WatchStart(string processName, Action<DateTime> notifyAction)
        {
            return new WMIWatcher("Win32_ProcessStartTrace", processName, notifyAction);
        }
        public IWMIWatcher WatchStop(string processName, Action<DateTime> notifyAction)
        {
            return new WMIWatcher("Win32_ProcessStopTrace", processName, notifyAction);
        }
    }
}
