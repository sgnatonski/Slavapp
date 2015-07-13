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
using System.IO;
using System.Windows.Resources;

namespace SlavApp.Minion.ImageFinder
{
    public class ImgConverter : IValueConverter
    {
        private static ImageCache imgCache = new ImageCache();
        private static ImageCache thumbImgCache = new ImageCache();
        private static StreamResourceInfo sri;
        private static BitmapImage noImageBmp;
        private static BitmapImage NoImageBmp
        {
           get
           {
               if (noImageBmp == null)
               {
                   sri = Application.GetResourceStream(new Uri("/SlavApp.Minion.ImageFinder;component/Resources/NoImage.jpg", UriKind.Relative));
                   noImageBmp = new BitmapImage();
                   noImageBmp.BeginInit();
                   noImageBmp.StreamSource = sri.Stream;
                   noImageBmp.EndInit();
               }

               return noImageBmp;
           }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                BitmapImage bmp = null;
                bool thumb = bool.Parse(parameter.ToString());
                if (thumb)
                {
                    if (!thumbImgCache.TryGetValue(value.ToString(), out bmp) || bmp == null)
                    {
                        try
                        {
                            bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource = new Uri(value.ToString());
                            bmp.DecodePixelWidth = 100;
                            bmp.EndInit();

                            thumbImgCache.Add(value.ToString(), bmp);
                        }
                        catch (FileFormatException)
                        {
                            bmp = noImageBmp;
                        }
                        catch (NotSupportedException)
                        {
                            bmp = noImageBmp;
                        }
                    }
                }
                else
                {
                    if (!imgCache.TryGetValue(value.ToString(), out bmp) || bmp == null)
                    {
                        try
                        {
                            bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource = new Uri(value.ToString());
                            bmp.EndInit();

                            imgCache.Add(value.ToString(), bmp);
                        }
                        catch (FileFormatException)
                        {
                            bmp = noImageBmp;
                        }
                        catch (NotSupportedException)
                        {
                            bmp = noImageBmp;
                        }
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