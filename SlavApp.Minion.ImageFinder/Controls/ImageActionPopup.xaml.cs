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

namespace SlavApp.Minion.ImageFinder.Controls
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
            var model = (SimilarityModel)this.DataContext;
            model.ShowImage();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var model = (SimilarityModel)this.DataContext;
            model.DeleteImage();
        }
    }
}
