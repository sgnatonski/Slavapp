using Caliburn.Micro;
using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SlavApp.Minion.ImageCompressor.ViewModels
{
    public class MainViewModel : Screen, IHandle<CancelProgressMessage>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly Timer eventTimer = new Timer();
        public MainViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);

            this.DirectoryName = @"R:\APART_ALL\ZDJĘCIA EXPO";
            eventTimer.Interval = 125;
            eventTimer.Elapsed += eventTimer_Elapsed;
        }

        protected override void OnActivate()
        {
            progressVM = (IProgressViewModel)this.Parent;
            base.OnActivate();
        }

        private IProgressViewModel progressVM;
            
        private string directoryName;
        public string DirectoryName
        {
            get { return directoryName; }
            set
            {
                this.directoryName = value;
                NotifyOfPropertyChange(() => DirectoryName);
            }
        }

        public void SelectDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.DirectoryName = dialog.SelectedPath;
            }
        }

        private void eventTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(true);
        }

        public void Handle(CancelProgressMessage message)
        {
        }
    }
}
