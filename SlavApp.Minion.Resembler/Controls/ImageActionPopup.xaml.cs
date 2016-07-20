using Caliburn.Micro;
using SlavApp.Minion.Resembler;
using SlavApp.Minion.Resembler.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SlavApp.Minion.Resembler.Controls
{
    /// <summary>
    /// Interaction logic for ImageActionPopup.xaml
    /// </summary>
    public partial class ImageActionPopup : Popup
    {
        public ImageActionPopup()
        {
            InitializeComponent();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var model = (SimilarityViewModel)this.DataContext;
            model.ShowImage();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var model = (SimilarityViewModel)this.DataContext;
            var messageBoxResult = MessageBox.Show(string.Format("Are you sure you want to delete\n\n{0}?\n\nIt will be moved to trash bin.", model.Name), "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                model.DeleteImage();
                this.IsOpen = false;
            } 
        }
    }
}
