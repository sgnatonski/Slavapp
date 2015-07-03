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
    public class MainViewModel : Screen, IHandle<CancelProgressMessage>
    {
        private readonly SimilarityRunAction sAction;
        private readonly IEventAggregator eventAggregator;
        private readonly Timer eventTimer = new Timer();
        public MainViewModel(IEventAggregator eventAggregator, SimilarityRunAction sAction)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
            this.sAction = sAction;
            this.sAction.OnPrepareProgress += OnPrepareProgress;
            this.sAction.OnCompareProgress += OnRunProgress;
            this.sAction.Completed += a_Completed;

            this.DirectoryName = @"R:\APART_ALL\ZDJĘCIA EXPO";
            this.SimLevel = 85;
            eventTimer.Interval = 125;
            eventTimer.Elapsed += eventTimer_Elapsed;
        }

        protected override void OnActivate()
        {
            progressVM = (IProgressViewModel)this.Parent;
            base.OnActivate();
        }

        private IProgressViewModel progressVM;
            
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
                var r = this.Results.Select(x => x.Value).OrderByDescending(x => x.Similar.Count).ToList();
                return r;
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

            c = 0;
            t = 1;
            eventTimer.Start();

            this.progressVM.ShowProgress();
            this.progressVM.UpdateProgress("[ 1 / 3 ] Starting analysis", 0, 1);
            this.sAction.DirectoryName = this.DirectoryName;
            this.sAction.SimilarityLevel = (double)this.SimLevel / 100.0;
            return this.sAction;
        }

        private async void a_Completed(object sender, ResultCompletionEventArgs e)
        {
            NotifyOfPropertyChange(() => this.List);
            this.sAction.CanRun = false;
            eventTimer.Stop();
            await this.progressVM.CloseProgress();
        }

        private long c = 0;
        private long t = 1;
        private bool preparing = true;

        private void OnPrepareProgress(object sender, PrepareEventArgs ea)
        {
            preparing = true;
            c++;
            t = ea.Total;
        }

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

        private void eventTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (c == 0)
            {
                return;
            }
            
            if (preparing)
            {
                this.progressVM.UpdateProgress(string.Format("[ 2 / 3 ] Preparing: {0:0.00} % ({1} / {2})", (c * 1.0 / t) * 100.0, c, t), c, t);
            }
            else
            {
                this.progressVM.UpdateProgress(string.Format("[ 3 / 3 ] Comparing: {0:0.00} % ({1} / {2})", (c * 1.0 / t) * 100.0, c, t), c, t);
            }
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

        public void Handle(CancelProgressMessage message)
        {
            this.sAction.CanRun = false;
        }
    }
}