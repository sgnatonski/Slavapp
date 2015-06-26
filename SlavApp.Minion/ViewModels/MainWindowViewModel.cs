using Caliburn.Micro;
using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive 
    {
        private readonly IPluginManager pluginManager;
        public MainWindowViewModel(IPluginManager pluginManager)
        {
            this.pluginManager = pluginManager;
            this.DisplayName = "Minion";
        }

        public List<IPlugin> Plugins
        {
            get
            {
                return this.pluginManager.List().ToList();
            }
        }

        public void ShowMain()
        {
            this.CloseItem(this.ActiveItem);
        }

        public void ShowPluginView(IPlugin plugin)
        {
            var vm = this.pluginManager.Create(plugin);
            this.ActivateItem(vm);
        }
    }
}
