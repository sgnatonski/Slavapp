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
using System.ComponentModel;
using System.Windows.Markup;
using System.Threading;

namespace SlavApp.Minion.Resembler.Converters
{
    public class ImgConverter : MarkupExtension, IValueConverter
    {
        private static ImageCache thumbImgCache = new ImageCache();        
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BitmapImage bmp = null;
            if (value != null)
            {
                var thumb = bool.Parse((string)parameter);
                var picName = value.ToString();
                if (thumb)
                {
                    bmp = thumbImgCache.GetOrAddToThumbnailCache(picName);
                }
                else
                {
                    bmp = thumbImgCache.GetImage(picName);
                }
            }

            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
 	        throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}