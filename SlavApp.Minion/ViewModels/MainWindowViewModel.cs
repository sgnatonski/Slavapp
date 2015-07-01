using Caliburn.Micro;
using SlavApp.Minion.Plugin;
using SlavApp.Minion.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive, IHandle<ProgressMessage>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IPluginManager pluginManager;

        public MainWindowViewModel(IEventAggregator eventAggregator, IPluginManager pluginManager)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
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
            this.pluginManager.CloseAll();
            this.CloseItem(this.ActiveItem);
        }

        public void ShowPluginView(IPlugin plugin)
        {
            var vm = this.pluginManager.Create(plugin);
            this.ActivateItem(vm);
        }

        public void PublishCancelProgressMessage()
        {
            this.eventAggregator.PublishOnUIThread(new CancelProgressMessage());
        }

        public void Handle(ProgressMessage message)
        {
            ((MainWindowView)this.GetView()).Handle(message);
        }
    }
}
