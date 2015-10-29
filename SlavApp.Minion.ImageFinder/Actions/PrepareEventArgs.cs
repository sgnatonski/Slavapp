using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.ImageFinder.Actions
{
    public class PrepareEventArgs : EventArgs
    {
        public PrepareEventArgs(long total)
        {
            Total = total;
        }
        public long Total { get; private set; }
    }
}
