using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SlavApp.Minion.Resembler
{
    public class ImageCache
    {
        private ObservableConcurrentDictionary<string, BitmapImage> cache = new ObservableConcurrentDictionary<string, BitmapImage>();

        private static BitmapImage noImageBmp;
        public BitmapImage NoImageBmp
        {
            get
            {
                if (noImageBmp == null)
                {
                    var sri = Application.GetResourceStream(new Uri("/SlavApp.Minion.Resembler;component/Resources/NoImage.jpg", UriKind.Relative));
                    noImageBmp = new BitmapImage();
                    noImageBmp.BeginInit();
                    noImageBmp.StreamSource = sri.Stream;
                    noImageBmp.CacheOption = BitmapCacheOption.OnLoad;
                    noImageBmp.EndInit();
                    noImageBmp.Freeze();
                }

                return noImageBmp;
            }
        }

        public bool IsImageCached(string image)
        {
            return this.cache.ContainsKey(image);
        }

        public BitmapImage GetOrAddToThumbnailCache(string filename)
        {
            BitmapImage bmp = null;
            if (!this.cache.TryGetValue(filename, out bmp) || bmp == null)
            {
                try
                {
                    bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(filename);
                    bmp.DecodePixelWidth = 80;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();

                    this.cache.Add(filename, bmp);
                }
                catch (FileFormatException)
                {
                    bmp = this.NoImageBmp;
                }
                catch (NotSupportedException)
                {
                    bmp = this.NoImageBmp;
                }
            }
            return bmp;
        }

        public BitmapImage GetImage(string filename)
        {
            BitmapImage bmp = null;
            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(filename);
                bmp.DecodePixelWidth = 600;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();

                this.cache.Add(filename, bmp);
            }
            catch (FileFormatException)
            {
                bmp = this.NoImageBmp;
            }
            catch (NotSupportedException)
            {
                bmp = this.NoImageBmp;
            }
            return bmp;
        }

        public void PurgeThumbCache()
        {
            cache = new ObservableConcurrentDictionary<string, BitmapImage>();
        }
    }
}
