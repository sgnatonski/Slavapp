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
    public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive, IProgressViewModel, IHandle<OpenPluginMessage>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IPluginManager pluginManager;
        private ProgressDialogController controller;

        public MainWindowViewModel(IEventAggregator eventAggregator, IPluginManager pluginManager)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
            this.pluginManager = pluginManager;
            this.DisplayName = "Minion";
        }

        public bool IsPluginVisible
        {
            get
            {
                if (this.ActiveItem == null)
                {
                    return false;
                }
                return this.Plugins.Any(p => this.ActiveItem.DisplayName == p.EntryViewModelType.FullName);
            }
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
            NotifyOfPropertyChange(() => this.IsPluginVisible);
        }

        public void ShowPluginView(IPlugin plugin)
        {
            var vm = this.pluginManager.CreateEntryViewModel(plugin);
            this.ActivateItem(vm);
            this.DisplayName = "Minion - " + plugin.Name;
            NotifyOfPropertyChange(() => this.IsPluginVisible);
        }

        public void ShowPluginSettings()
        {
            var plugin = this.Plugins.FirstOrDefault(p => this.ActiveItem.DisplayName == p.EntryViewModelType.FullName);
            if (plugin != null)
            {
                var vm = this.pluginManager.CreateSettingsViewModel(plugin);
                this.ActivateItem(vm);
            }
            else
            {
                this.ActiveItem.TryClose();
            }
        }

        bool progressWasClosed;
        public async Task ShowProgress()
        {
            this.controller = await ((MainWindowView)this.GetView()).OpenProgress().ConfigureAwait(false);
            this.controller.Maximum = int.MaxValue;
            this.controller.SetIndeterminate();
            this.controller.SetCancelable(true);
            if (progressWasClosed)
            {
                progressWasClosed = false;
                await this.controller.CloseAsync().ConfigureAwait(false);
            }
        }

        public Task UpdateProgress(string message, double current, double total)
        {
            if (this.controller != null)
            {
                if (this.controller.IsCanceled)
                {
                    this.eventAggregator.PublishOnUIThread(new CancelProgressMessage());
                }
                else if (controller.IsOpen)
                {
                    this.controller.SetMessage(message);
                    this.controller.Maximum = total;
                    if (current <= total)
                    {
                        this.controller.SetProgress(current);
                    }
                }
            }
            return Task.FromResult(0);
        }
        public async Task CloseProgress()
        {
            if (this.controller != null && this.controller.IsOpen)
            {
                await this.controller.CloseAsync().ConfigureAwait(false);
            }
            else
            {
                progressWasClosed = true;
            }
        }

        public void Handle(OpenPluginMessage message)
        {
            var plugin = this.Plugins.FirstOrDefault(p => p.EntryViewModelType.FullName == message.Plugin.EntryViewModelType.FullName);
            if (plugin != null)
            {
                this.ShowPluginView(plugin);
            }
        }
    }
}
