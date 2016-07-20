using Caliburn.Micro;
using System;
using System.Collections.Generic;
namespace SlavApp.Minion.Plugin
{
    public interface IPluginManager
    {
        IEnumerable<IPlugin> List();
        IScreen CreateEntryViewModel(IPlugin plugin);
        IScreen CreateSettingsViewModel(IPlugin plugin);
        void CloseAll();
    }
}
