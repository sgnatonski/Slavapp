using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SlavApp.Minion.Plugin
{
    public class ProgressMessage
    {
        public bool IsInitial { get; set; }
        public bool IsFinal { get; set; }
        public long Current { get; set; }
        public long Total { get; set; }
        public string Message { get; set; }
        public string SubMessage { get; set; }
    }
}