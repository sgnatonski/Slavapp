﻿using Caliburn.Micro;
using ImageFinder;
using OxyPlot;
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
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SlavApp.Minion.ImageFinder.ViewModels
{
    public class MainViewModel : Screen
    {
        private readonly SimilarityRunAction sAction;
        Timer timer = new Timer();
        Timer eventTimer = new Timer();
        public MainViewModel(SimilarityRunAction sAction)
        {
            this.sAction = sAction;
            this.sAction.OnPrepareProgress += OnPrepareProgress;
            this.sAction.OnCompareProgress += OnRunProgress;
            this.sAction.Completed += a_Completed;
            
            this.DirectoryName = @"R:\APART_ALL\ZDJĘCIA EXPO";
            this.SimLevel = 90;
            this.PlotModel = new PlotModel();
            this.PlotModel.Series.Add(new OxyPlot.Series.LineSeries());
            timer.Interval = 1000;
            timer.Elapsed += timer_Elapsed;
            eventTimer.Interval = 250;
            eventTimer.Elapsed += eventTimer_Elapsed;
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

        private long current;
        public long Current
        {
            get { return current; }
            set
            {
                this.current = value;
                NotifyOfPropertyChange(() => Current);
            }
        }

        private long maximum;
        public long Maximum
        {
            get { return maximum; }
            set
            {
                this.maximum = value;
                NotifyOfPropertyChange(() => Maximum);
            }
        }

        private int simLevel;
        public int SimLevel
        {
            get { return simLevel; }
            set
            {
                this.simLevel = value;
                NotifyOfPropertyChange(() => SimLevel);
            }
        }

        private ObservableConcurrentDictionary<string, ResultViewModel> results;
        public ObservableConcurrentDictionary<string, ResultViewModel> Results
        {
            get { return this.results; }
            set
            {
                this.results = value;
                NotifyOfPropertyChange(() => Results);
                NotifyOfPropertyChange(() => List);
            }
        }

        public List<ResultViewModel> List
        {
            get
            {
                if (Results == null)
                {
                    return Enumerable.Empty<ResultViewModel>().ToList();
                }
                return this.Results.OrderByDescending(x => x.Value.SimilarCount).Select(x => x.Value).ToList();
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
            this.Results = new ObservableConcurrentDictionary<string, ResultViewModel>();
            this.Results.IsNotifying = false;
            this.Current = 0;
            this.Maximum = int.MaxValue;

            this.sAction.CanRun = true;
            this.sAction.DirectoryName = this.DirectoryName;
            this.sAction.SimilarityLevel = (double)this.SimLevel / 100.0;

            this.PlotModel.Series.OfType<OxyPlot.Series.LineSeries>().First().Points.Clear();
            x = 0;
            old = 0;
            timer.Start();
            eventTimer.Start();
            return this.sAction;
        }

        private long x = 0;
        private long old = 0;
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            x++;
            if (this.Current - old < 0)
            {
                old = 0;
                this.PlotModel.InvalidatePlot(false);
                return;
            }
            this.PlotModel.Series.OfType<OxyPlot.Series.LineSeries>().First().Points.Add(new DataPoint(x, this.Current - old));
            this.PlotModel.InvalidatePlot(true);
            old = this.Current;
        }

        void a_Completed(object sender, ResultCompletionEventArgs e)
        {
            Execute.BeginOnUIThread(() =>
            {
                this.Current = 0;
                this.Maximum = int.MaxValue;
                this.ProgressText = string.Empty;
            });
            NotifyOfPropertyChange(() => this.List);
            this.sAction.CanRun = false;
            timer.Stop();
            eventTimer.Stop();
        }

        private void OnPrepareProgress(object sender, PrepareEventArgs ea)
        {
            c++;
            t = ea.Total;
        }

        public PlotModel PlotModel { get; private set; }

        private long c = 0;
        private long t = 1;
        private bool preparing = true;

        private void OnRunProgress(object sender, SimilarityRunEventArgs ea)
        {
            if (preparing)
            {
                c = 0;
                preparing = false;
            }
            c++;
            t = ea.Total;
            if (ea.File1 != null)
            {
                if (!this.Results.ContainsKey(ea.File1))
                {
                    this.Results.Add(ea.File1, new ResultViewModel(new SimilarityModel() { Name = ea.File1 }));
                }
                this.Results[ea.File1].Similar.Add(new SimilarityModel() { Name = ea.File2, Value = ea.Value });
            }
        }

        void eventTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Execute.BeginOnUIThread(() =>
            {
                this.Maximum = t;
                this.Current = c;
                if (preparing)
                {
                    this.ProgressText = string.Format("Preparing: {0:0.00} % ({1} / {2})", (this.Current * 1.0 / t) * 100.0, this.Current, t);
                }
                else
                {
                    this.ProgressText = string.Format("Comparing: {0:0.00} % ({1} / {2})", (this.Current * 1.0 / t) * 100.0, this.Current, t);
                }
            });
        }

        public override void CanClose(Action<bool> callback)
        {
            if (!this.sAction.CanRun)
            {
                callback(true);
            }
            else
            {
                this.sAction.CanRun = false;
                this.sAction.Completed += (sender, ea) =>
                {
                    callback(true);
                };
            }
        }
    }
}
