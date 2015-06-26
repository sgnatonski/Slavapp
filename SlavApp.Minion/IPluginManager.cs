using Caliburn.Micro;
using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
namespace SlavApp.Minion
{
    public interface IPluginManager
    {
        IEnumerable<IPlugin> List();
        IScreen Create(IPlugin plugin);
    }
}
