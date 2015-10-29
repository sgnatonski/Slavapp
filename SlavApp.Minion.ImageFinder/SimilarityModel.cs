using ExifLib;
using SlavApp.Windows;
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
        private int value;
        private ObservableCollection<ExifData> exif;
        private bool exifLoaded;
        public string Name
        {
            get { return name; }
            set
            {
                if (this.name != value)
                {
                    this.exifLoaded = false;
                }
                this.name = value;
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => FileName);
                NotifyOfPropertyChange(() => Exif);
            }
        }
        public string FileName
        {
            get { return Path.GetFileNameWithoutExtension(this.name); }
        }
        public int Value
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
            get 
            { 
                if (!exifLoaded)
                {
                    exifLoaded = true;
                    UpdateExif();
                }
                return exif; 
            }
        }

        private void GetExifTag(ExifReader reader, ExifTags tag, string title, List<ExifData> list)
        {
            object value = null;
            if (reader.GetTagValue<object>(tag, out value))
            {
                list.Add(new ExifData() { Key = title, Value = value.ToString() });
            }
        }

        private void UpdateExif()
        {
            var e = new List<ExifData>();
            e.Add(new ExifData() { Key = "Distance", Value = this.Value.ToString() });
            try
            {
                using (var reader = new ExifReader(Name))
                {
                    GetExifTag(reader, ExifTags.ImageUniqueID, "ID", e);
                    GetExifTag(reader, ExifTags.DateTimeDigitized, "Date taken", e);
                    GetExifTag(reader, ExifTags.PixelXDimension, "Width", e);
                    GetExifTag(reader, ExifTags.PixelYDimension, "Height", e);
                    GetExifTag(reader, ExifTags.XResolution, "X Resolution", e);
                    GetExifTag(reader, ExifTags.YResolution, "Y Resolution", e);
                    GetExifTag(reader, ExifTags.Make, "Device", e);
                    GetExifTag(reader, ExifTags.Model, "Device model", e);
                }
            }
            catch(ExifLibException eex)
            {

            }
            this.exif = new ObservableCollection<ExifData>(e);
        }

        public void ShowImage()
        {
            Process.Start(this.Name);
        }

        public void DeleteImage()
        {
            FileOperationAPIWrapper.Send(this.Name);
        }
    }

    public class ExifData
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
