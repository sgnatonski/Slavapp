using Caliburn.Micro;
using SlavApp.Minion.Resembler.Messages;
using SlavApp.Minion.Resembler.ViewModels;
using SlavApp.Minion.Plugin;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlavApp.Minion.Resembler
{
    public class ResemblerPlugin : IPlugin
    {
        private readonly IEventAggregator eventAggregator;
        private DirectorySearchHandler directorySearchHandler;

        public ResemblerPlugin(IEventAggregator eventAggregator, DirectorySearchHandler directorySearchHandler)
        {
            this.eventAggregator = eventAggregator;
            this.directorySearchHandler = directorySearchHandler;
        }

        public string Name
        {
            get { return "Resembler"; }
        }

        public Type EntryViewModelType 
        { 
            get { return typeof(MainViewModel); }
        }

        public Type SettingsViewModelType
        {
            get { return typeof(SettingsViewModel); }
        }

        public string IconName
        {
            get { return "appbar.magnify.browse"; }
        }

        public void Startup()
        {
            this.directorySearchHandler.Initialize(this);
        }

        public void Cleanup()
        {
            this.directorySearchHandler.Dispose();
        }
    }
}
