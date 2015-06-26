using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SlavApp.Minion.ImageFinder
{
    public class ImageCache : ObservableConcurrentDictionary<string, BitmapImage>
    {
    }
}
