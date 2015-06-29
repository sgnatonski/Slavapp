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
        private readonly List<IScreen> activePlugins = new List<IScreen>();

        public PluginManager(Container container)
        {
            this.container = container;
        }

        public IEnumerable<IPlugin> List()
        {
            return this.container.GetAllInstances<IPlugin>();
        }

        public IScreen Create(IPlugin plugin)
        {
            var screen = (IScreen)this.container.GetInstance(plugin.EntryViewModelType);
            this.activePlugins.Add(screen);
            return screen;
        }

        public void CloseAll()
        {
            this.activePlugins.ForEach(p =>
            {
                p.TryClose();
            });

            this.activePlugins.Clear();
        }
    }
}
