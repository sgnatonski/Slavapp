using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
                NotifyOfPropertyChange(() => FileName);
            }
        }
        public string FileName
        {
            get { return Path.GetFileNameWithoutExtension(this.name); }
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

        public void ShowImage()
        {
            Process.Start(this.Name);
        }
    }
}
