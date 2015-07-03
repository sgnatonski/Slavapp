using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.ImageFinder.ViewModels
{
    public class ResultViewModel : PropertyChangedBase
    {
        public ResultViewModel(SimilarityModel model)
        {
            this.model = model;
            this.Similar = new BindableCollection<SimilarityModel>();
            this.Similar.CollectionChanged += Similar_CollectionChanged;
        }

        void Similar_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => SimilarCount);
        }

        private SimilarityModel model;

        public SimilarityModel Model
        {
            get { return model; }
            set
            {
                this.model = value;
                NotifyOfPropertyChange(() => Model);
            }
        }

        private BindableCollection<SimilarityModel> similar;

        public BindableCollection<SimilarityModel> Similar
        {
            get { return similar; }
            set
            {
                this.similar = value;
                NotifyOfPropertyChange(() => Similar);
                NotifyOfPropertyChange(() => SimilarCount);
            }
        }

        public string SimilarCount
        {
            get { return "+" + this.Similar.Count; }
        }
    }
}
