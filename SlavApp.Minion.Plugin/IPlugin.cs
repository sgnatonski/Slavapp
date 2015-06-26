using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        Type EntryViewModelType { get; }
    }
}
