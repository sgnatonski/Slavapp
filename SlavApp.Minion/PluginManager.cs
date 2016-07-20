using Caliburn.Micro;
using SimpleInjector;
using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion
{
    public sealed class PluginManager : IPluginManager
    {
        private readonly Container container;
        private List<IPlugin> plugins;
        private readonly List<IScreen> activeScreens = new List<IScreen>();

        public PluginManager(Container container)
        {
            this.container = container;
        }

        public IEnumerable<IPlugin> List()
        {
            if (plugins == null)
            {
                plugins = this.container.GetAllInstances<IPlugin>().ToList();
                plugins.ForEach(x => x.Startup());
            }
            return plugins;
        }

        public IScreen CreateEntryViewModel(IPlugin plugin)
        {
            var screen = (IScreen)this.container.GetInstance(plugin.EntryViewModelType);
            this.activeScreens.Add(screen);
            return screen;
        }
        public IScreen CreateSettingsViewModel(IPlugin plugin)
        {
            var screen = (IScreen)this.container.GetInstance(plugin.SettingsViewModelType);
            this.activeScreens.Add(screen);
            return screen;
        }

        public void CloseAll()
        {
            this.activeScreens.ForEach(p =>
            {
                p.TryClose();
            });

            this.activeScreens.Clear();
        }
    }
}
