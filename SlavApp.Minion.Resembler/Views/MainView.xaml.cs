using SlavApp.Minion.Resembler.Controls;
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

namespace SlavApp.Minion.Resembler.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        public void ScrollToTop()
        {
            var scrollViewer = (ViewportAwareScrollViewer)this.List.Template.FindName("viewport", this.List);
            if (scrollViewer != null)
            {
                scrollViewer.InvalidateScrollInfo();
            }            
        }
    }
}
