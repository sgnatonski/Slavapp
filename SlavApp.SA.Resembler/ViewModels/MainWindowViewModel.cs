using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using SlavApp.Minion.Plugin;
using SlavApp.Minion.Resembler.ViewModels;
using SlavApp.Minion.Views;
using SlavApp.SA.Resembler.ViewModels;
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
        private readonly IWindowManager windowManager;
        private ProgressDialogController controller;
        private readonly SettingsViewModel settingsVM;
        private readonly StatusViewModel statusVM;
        private readonly MainViewModel mainVM;
        private readonly InfoViewModel infoVM;

        public MainWindowViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, SettingsViewModel settingsVM, StatusViewModel statusVM, MainViewModel mainVM, InfoViewModel infoVM)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
            this.windowManager = windowManager;
            this.settingsVM = settingsVM;
            this.statusVM = statusVM;
            this.mainVM = mainVM;
            this.infoVM = infoVM;
            this.DisplayName = "Resembler";

            this.statusVM.ActivateWith(mainVM);
            this.ActivateItem(mainVM);
        }

        public StatusViewModel StatusItem
        {
            get { return this.statusVM; }
        }

        public bool IsSettingVisible
        {
            get
            {
                return this.ActiveItem == settingsVM;
            }
        }

        public bool IsMainVisible
        {
            get
            {
                return this.ActiveItem == mainVM;
            }
        }

        public void ShowMain()
        {
            this.ActivateItem(mainVM);
            NotifyOfPropertyChange(() => this.IsMainVisible);
            NotifyOfPropertyChange(() => this.IsSettingVisible);
        }

        public void ShowPluginSettings()
        {
            this.ActivateItem(settingsVM);
            NotifyOfPropertyChange(() => this.IsMainVisible);
            NotifyOfPropertyChange(() => this.IsSettingVisible);
        }

        bool progressWasClosed;
        public async Task ShowProgress()
        {
            var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
            prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal);
            this.controller = await ((MainWindowView)this.GetView()).OpenProgress().ConfigureAwait(false);
            this.controller.Maximum = int.MaxValue;
            this.controller.SetCancelable(true);
            if (progressWasClosed)
            {
                progressWasClosed = false;
                await this.controller.CloseAsync().ConfigureAwait(false);
            }
        }

        public Task UpdateProgress(string message, double current, double total)
        {
            var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
            prog.SetProgressValue((int)current, (int)total);
            
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
            var prog = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;
            prog.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress);
            if (this.controller != null && this.controller.IsOpen)
            {
                await this.controller.CloseAsync().ConfigureAwait(false);
            }
            else
            {
                progressWasClosed = true;
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            var result = windowManager.ShowDialog(infoVM);
            if (result.GetValueOrDefault())
                callback(true);
        }
    }
}
