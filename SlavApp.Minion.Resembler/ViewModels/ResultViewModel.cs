using Caliburn.Micro;
using SlavApp.Minion.Resembler.Messages;
using SlavApp.Resembler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SlavApp.Minion.Resembler.ViewModels
{
    public class ResultViewModel : PropertyChangedBase
    {
        private BindableCollection<SimilarityViewModel> similar;
        private readonly IEventAggregator eventAggregator;
        public ResultViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            this.Similar = new BindableCollection<SimilarityViewModel>();
        }

        public void AddDistances(string resultFilename, IEnumerable<Distance> distances)
        {
            this.Similar = new BindableCollection<SimilarityViewModel>(
                distances.Select(s => new SimilarityViewModel(this.eventAggregator)
                {
                    Name = s.Filename2,
                    Value = s.DistanceBetween
                }).Concat(new[] 
                {
                    new SimilarityViewModel(this.eventAggregator)
                    {
                        Name = resultFilename,
                        Value = -1
                    }
                }).OrderBy(s => s.Value));
        }
        
        public BindableCollection<SimilarityViewModel> Similar
        {
            get { return similar; }
            private set
            {
                this.similar = value;
                NotifyOfPropertyChange(() => Similar);
            }
        }

        public string SimilarCount
        {
            get { return "+" + (this.Similar.Count); }
        }

        public string CenterModelName
        {
            get { return this.Similar.Select(x => x.Name).FirstOrDefault(); }
        }

        public string ModelName1
        {
            get { return this.Similar.Skip(1).Select(x => x.Name).FirstOrDefault(); }
        }

        public string ModelName2
        {
            get { return this.Similar.Skip(2).Select(x => x.Name).FirstOrDefault(); }
        }

        public string ModelName3
        {
            get { return this.Similar.Skip(3).Select(x => x.Name).FirstOrDefault(); }
        }

        /*private static ImageCache imgCache = new ImageCache();
        
        private BitmapSource centerBmp;
        public BitmapSource CenterBmp
        {
            get
            {
                if (centerBmp == null)
                {
                    LoadImageAsync(b => CenterBmp = b, CenterModelName);
                }
                return centerBmp;
            }
            set { centerBmp = value; NotifyOfPropertyChange(() => CenterBmp); }
        }

        private BitmapSource bmp1;
        public BitmapSource Bmp1
        {
            get
            {
                if (bmp1 == null && ModelName1 != null)
                {
                    LoadImageAsync(b => Bmp1 = b, ModelName1);
                }
                return bmp1;
            }
            set { bmp1 = value; NotifyOfPropertyChange(() => Bmp1); }
        }

        private BitmapSource bmp2;
        public BitmapSource Bmp2
        {
            get
            {
                if (bmp2 == null && ModelName2 != null)
                {
                    LoadImageAsync(b => Bmp2 = b, ModelName2);
                }
                return bmp2;
            }
            set { bmp2 = value; NotifyOfPropertyChange(() => Bmp2); }
        }

        private BitmapSource bmp3;
        public BitmapSource Bmp3
        {
            get
            {
                if (bmp3 == null && ModelName3 != null)
                {
                    LoadImageAsync(b => Bmp3 = b, ModelName3);
                }
                return bmp3;
            }
            set { bmp3 = value; NotifyOfPropertyChange(() => Bmp3); }
        }*/

        private void LoadImageAsync(Action<BitmapSource> src, string imageName)
        {
            //Debug.WriteLine(imageName);
            //if (imgCache.IsImageCached(imageName))
            {
                //Dispatcher.CurrentDispatcher.BeginInvoke(new System.Action(() => { src(imgCache.GetOrAddToThumbnailCache(imageName)); }));                
            }
           /* else 
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(new System.Action(() =>
                {
                    Task.Run(() => src(imgCache.GetOrAddToThumbnailCache(imageName)));
                }));
            } */       
        }
    }
}
