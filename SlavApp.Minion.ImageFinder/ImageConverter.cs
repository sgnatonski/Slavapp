using Caliburn.Micro;
using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows;

namespace SlavApp.Minion.ImageFinder
{
    public class ImgConverter : IValueConverter
    {
        private static ImageCache imgCache = new ImageCache();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                BitmapImage bmp = null;
                if (!imgCache.TryGetValue(value.ToString(), out bmp) || bmp == null)
                {
                    try
                    {
                        bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(value.ToString());
                        bmp.DecodePixelWidth = 100;
                        bmp.EndInit();

                        imgCache.Add(value.ToString(), bmp);
                    }
                    catch (NotSupportedException)
                    {
                        var sri = Application.GetResourceStream(new Uri("/SlavApp.Minion.ImageFinder;component/Resources/NoImage.jpg", UriKind.Relative));
                        bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.StreamSource = sri.Stream;
                        bmp.EndInit();
                    }
                }
                return bmp;
            }
            else
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
 	        throw new NotImplementedException();
        }
    }
}