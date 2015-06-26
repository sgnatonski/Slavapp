using Caliburn.Micro;
using ImageFinder;
using SlavApp.Minion.ImageFinder.Actions;
using SlavApp.Minion.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SlavApp.Minion.ImageFinder.ViewModels
{
    public class MainViewModel : Screen
    {
        private readonly SimilarityRunAction sAction;
        public MainViewModel(SimilarityRunAction sAction)
        {
            this.sAction = sAction;
            this.sAction.OnProgress += OnRunProgress;

            this.DirectoryName = @"R:\APART_ALL\ZDJĘCIA EXPO";
        }

        private string directoryName;
        public string DirectoryName
        {
            get { return directoryName; }
            set
            {
                this.directoryName = value;
                NotifyOfPropertyChange(() => DirectoryName);
            }
        }

        private string progressText;
        public string ProgressText
        {
            get { return progressText; }
            set
            {
                this.progressText = value;
                NotifyOfPropertyChange(() => ProgressText);
            }
        }

        private int current;
        public int Current
        {
            get { return current; }
            set
            {
                this.current = value;
                NotifyOfPropertyChange(() => Current);
            }
        }

        private int maximum;
        public int Maximum
        {
            get { return maximum; }
            set
            {
                this.maximum = value;
                NotifyOfPropertyChange(() => Maximum);
            }
        }

        private ObservableConcurrentDictionary<string, BindableCollection<SimilarityModel>> results;
        public ObservableConcurrentDictionary<string, BindableCollection<SimilarityModel>> Results
        {
            get { return this.results; }
            set
            {
                this.results = value;
                NotifyOfPropertyChange(() => Results);
            }
        }

        public void SelectDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.DirectoryName = dialog.SelectedPath;
            }
        }

        public IResult Run()
        {
            this.Results = new ObservableConcurrentDictionary<string, BindableCollection<SimilarityModel>>();
            this.Results.IsNotifying = false;
            this.Current = 0;
            this.Maximum = int.MaxValue;

            var a = new SimilarityRunAction(new SimilarFinder());
            a.OnProgress += OnRunProgress;
            a.DirectoryName = this.DirectoryName;
            a.Completed += a_Completed;
            return a;
        }

        public void ShowImage(string filename)
        {
            Process.Start(filename);
        }

        void a_Completed(object sender, ResultCompletionEventArgs e)
        {
            Execute.BeginOnUIThread(() =>
            {
                this.Current = 0;
                this.Maximum = int.MaxValue;
                this.ProgressText = string.Empty;
            });
            this.Results.IsNotifying = true;
        }

        private void OnRunProgress(object sender, SimilarityRunEventArgs ea)
        {
            Execute.BeginOnUIThread(() =>
            {
                this.Maximum = ea.Total;
                this.Current++;
                this.ProgressText = string.Format("{0:0.00} % ({1} / {2})", (this.Current * 1.0 / ea.Total) * 100.0, this.Current, ea.Total);
            });
            if (ea.File1 != null)
            {
                if (!this.Results.ContainsKey(ea.File1))
                {
                    this.Results.Add(ea.File1, new BindableCollection<SimilarityModel>());
                }
                this.Results[ea.File1].Add(new SimilarityModel() { Name = ea.File2, Value = ea.Value });
            }
        }
    }
}
