using Caliburn.Micro;
using System;
using System.Collections.Generic;
namespace SlavApp.Minion.Plugin
{
    public interface IPluginManager
    {
        IEnumerable<IPlugin> List();
        IScreen Create(IPlugin plugin);
    }
}
