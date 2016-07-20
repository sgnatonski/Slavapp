using SlavApp.Minion.ImageCompressor.ViewModels;
using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.ImageCompressor
{
    public class ImageCompressorPlugin : IPlugin
    {
        public string Name
        {
            get { return "Image Compressor"; }
        }

        public Type EntryViewModelType
        {
            get { return typeof(MainViewModel); }
        }

        public Type SettingsViewModelType
        {
            get { return null; }
        }

        public string IconName
        {
            get { return "appbar.resource.group"; }
        }

        public void Startup()
        {
        }

        public void Cleanup()
        {
        }
    }
}
