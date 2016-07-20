using Caliburn.Micro;
using SlavApp.Minion.Resembler.Actions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SlavApp.Windows;

namespace SlavApp.Minion.Resembler.ViewModels
{
    public class SettingsViewModel : Screen
    {
        private readonly InstallContextAction iAction;
        private readonly JsonSettings.Settings settings;

        public SettingsViewModel(InstallContextAction iAction, IPathProvider pathProvider)
        {
            this.iAction = iAction;
            
            this.settings = new JsonSettings.Settings(Path.Combine(pathProvider.BasePath, "settings.json"));

            this.DrivesToScan = new ObservableCollection<DriveSelectionModel>(LoadDrives());
        }

        private IEnumerable<DriveSelectionModel> LoadDrives()
        {
            var drives = Directory.GetLogicalDrives();
            var d = settings["drives"];
            var sel = drives;
            if (d != "drives")
            {
                sel = d.Split(';');
            }
            var drivesToScan = Directory.GetLogicalDrives().Select(x => new DriveSelectionModel() { Drive = x, IsSelected = sel.Contains(x) }).ToList();
            drivesToScan.ForEach(drive => drive.PropertyChanged += new PropertyChangedEventHandler(drive_PropertyChanged));
            return drivesToScan;
        }

        private void drive_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            settings["drives"] = string.Join(";", this.DrivesToScan.Where(x => x.IsSelected).Select(x => x.Drive));
            settings.Save();
        }

        public IEnumerable<IResult> InstallContext()
        {
            iAction.Settings = this.settings;
            yield return iAction;
            NotifyOfPropertyChange(() => InstallContextLabel);
        }

        public string InstallContextLabel
        {
            get
            {
                if (settings["context"] == "1")
                {
                    return "Uninstall";
                }
                else
                {
                    return "Install";
                }
            }
        }

        public bool UsePHash
        {
            get { return settings["algorithm"] == "pHash"; }
            set
            {
                settings.ChangeSetting("algorithm", value ? "pHash" : "dHash");
                settings.Save();
                NotifyOfPropertyChange(() => UsePHash);
                NotifyOfPropertyChange(() => UseDHash);
            }
        }

        public bool UseDHash
        {
            get { return !this.UsePHash; }
            set
            {
                this.UsePHash = !value;
            }
        }

        private ObservableCollection<DriveSelectionModel> drivesToScan;
        public ObservableCollection<DriveSelectionModel> DrivesToScan
        {
            get { return drivesToScan; }
            set
            {
                drivesToScan = value;
                NotifyOfPropertyChange(() => DrivesToScan);
            }
        }
    }
}
