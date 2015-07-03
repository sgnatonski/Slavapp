using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using SlavApp.Minion.Plugin;
using SlavApp.Minion.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SlavApp.Minion.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive, IProgressViewModel
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IPluginManager pluginManager;
        private ProgressDialogController controller;

        public MainWindowViewModel(IEventAggregator eventAggregator, IPluginManager pluginManager)
        {
            this.eventAggregator = eventAggregator;
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
            this.DisplayName = "Minion";
        }

        public void ShowPluginView(IPlugin plugin)
        {
            var vm = this.pluginManager.Create(plugin);
            this.ActivateItem(vm);
            this.DisplayName = "Minion - " + plugin.Name;
        }

        public async Task ShowProgress()
        {
            this.controller = await ((MainWindowView)this.GetView()).OpenProgress();
            this.controller.Maximum = int.MaxValue;
            this.controller.SetCancelable(true);
        }

        public async Task UpdateProgress(string message, double current, double total)
        {
            if (this.controller != null)
            {
                if (this.controller.IsCanceled)
                {
                    this.eventAggregator.PublishOnUIThread(new CancelProgressMessage());
                    await this.controller.CloseAsync();
                    this.controller = null;
                }
                else
                {
                    this.controller.SetMessage(message);
                    this.controller.Maximum = total;
                    if (current <= total)
                    {
                        this.controller.SetProgress(current);
                    }
                }
            }
        }
        public async Task CloseProgress()
        {
            if (this.controller != null)
            {
                await this.controller.CloseAsync();
            }
        }
    }
}
