using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SlavApp.Minion.ImageFinder
{
    public class SimilarityModel : Caliburn.Micro.PropertyChangedBase
    {
        private string name;
        private double value;
        public string Name
        {
            get { return name; }
            set
            {
                this.name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
        public double Value
        {
            get { return value; }
            set
            {
                this.value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }
    }
}
