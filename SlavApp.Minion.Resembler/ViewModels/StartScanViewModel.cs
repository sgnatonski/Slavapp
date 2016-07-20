using Caliburn.Micro;
using System.IO;
using SlavApp.Windows;

namespace SlavApp.Minion.Resembler.ViewModels
{
    public class StartScanViewModel : Screen
    {
        private readonly JsonSettings.Settings settings;

        public StartScanViewModel(IPathProvider pathProvider)
        {            
            this.settings = new JsonSettings.Settings(Path.Combine(pathProvider.BasePath, "settings.json"));
            this.DisplayName = "Resembler";
        }

        public void RunScan()
        {
            TryClose(true);
        }

        public void SkipScan()
        {
            TryClose(false);
        }
    }
}
