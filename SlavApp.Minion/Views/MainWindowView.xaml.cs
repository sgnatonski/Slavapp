using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using Caliburn.Micro;
using SlavApp.Minion.Plugin;
using System.Threading;
using SlavApp.Minion.ViewModels; 

namespace SlavApp.Minion.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : MetroWindow
    {
        private ProgressDialogController pgController;

        public MainWindowView()
        {
            InitializeComponent();
        }

        public async void Handle(ProgressMessage message)
        {
            if (message.IsInitial)
            {
                pgController = await this.ShowProgressAsync("Please wait...", message.Message);
                pgController.Maximum = int.MaxValue;
                pgController.SetCancelable(true);
            }

            if (pgController == null)
            {
                return;
            }

            if (pgController.IsCanceled)
            {
                ((MainWindowViewModel)this.DataContext).PublishCancelProgressMessage();
                await pgController.CloseAsync(); 
                pgController = null;
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    pgController.SetMessage(message.Message);
                    pgController.Maximum = message.Total;
                    pgController.SetProgress(message.Current);
                });

                if (message.IsFinal)
                {
                    await pgController.CloseAsync();
                }
            }
        }
    }
}
