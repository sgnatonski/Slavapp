using Caliburn.Micro;
using SlavApp.Minion.ImageFinder.ViewModels;
using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.ImageFinder
{
    public class ImageFinderPlugin : IPlugin
    {
        public string Name
        {
            get { return "Image Finder"; }
        }

        public Type EntryViewModelType 
        { 
            get { return typeof(MainViewModel); }
        }


        public string IconName
        {
            get { return "appbar.magnify.browse"; }
        }
    }
}
