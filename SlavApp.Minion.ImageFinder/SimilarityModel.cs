using ExifLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<ExifData> exif;
        public string Name
        {
            get { return name; }
            set
            {
                this.name = value;
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => FileName);
                UpdateExif();
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

        public ObservableCollection<ExifData> Exif
        {
            get { return exif; }
            set
            {
                this.exif = value;
                NotifyOfPropertyChange(() => Exif);
            }
        }

        private void UpdateExif()
        {
            var e = new List<ExifData>();
            try
            {
                /*using (var reader = new ExifReader(Name))
                {
                    // Extract the tag data using the ExifTags enumeration
                    DateTime datePictureTaken;
                    if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out datePictureTaken))
                    {
                        e.Add(new ExifData() { Key = "Date taken", Value = datePictureTaken.ToString() });
                    }
                    double imgWidth;
                    if (reader.GetTagValue<double>(ExifTags.XResolution, out imgWidth))
                    {
                        e.Add(new ExifData() { Key = "Width", Value = imgWidth.ToString() });
                    }
                    double imgHeight;
                    if (reader.GetTagValue<double>(ExifTags.YResolution, out imgHeight))
                    {
                        e.Add(new ExifData() { Key = "Height", Value = imgHeight.ToString() });
                    }
                }*/
            }
            catch(ExifLibException eex)
            {

            }
            this.Exif = new ObservableCollection<ExifData>(e);
        }

        public void ShowImage()
        {
            Process.Start(this.Name);
        }
    }

    public class ExifData
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
