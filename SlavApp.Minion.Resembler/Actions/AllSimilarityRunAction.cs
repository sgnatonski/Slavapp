using Caliburn.Micro;
using SlavApp.Minion.Plugin;
using SlavApp.Minion.Resembler.ViewModels;
using SlavApp.Resembler;
using SlavApp.Resembler.DHash;
using SlavApp.Resembler.PHash;
using SlavApp.Windows;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlavApp.Minion.Resembler.Actions
{
    public class AllSimilarityRunAction : IResult<List<ResultViewModel>>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly DHashCalculator dhash_simFinder;
        private readonly DHashComparer dhash_simComparer;
        private readonly PHashCalculator phash_simFinder;
        private readonly PHashComparer phash_simComparer;

        private ConcurrentBag<ResultViewModel> results = new ConcurrentBag<ResultViewModel>();
        private int current;
        private int total;

        public AllSimilarityRunAction(IEventAggregator eventAggregator, DHashCalculator dsimFinder, DHashComparer dsimComparer, PHashCalculator psimFinder, PHashComparer psimComparer)
        {
            this.eventAggregator = eventAggregator;
            this.dhash_simFinder = dsimFinder;
            this.dhash_simComparer = dsimComparer;
            this.phash_simFinder = psimFinder;
            this.phash_simComparer = psimComparer;
        }

        public string Algorithm { get; set; }
        public int MaxDistance { get; set; }
        public bool CanRun { get; set; }

        public IProgressViewModel ProgressViewModel { get; set; }

        public List<ResultViewModel> Result
        {
            get
            {
                var g = results.Select(x => new { h = string.Join("", x.Similar.Select(s => s.Name).OrderBy(s => s)), v = x })
                     .GroupBy(x => x.h);
                return g.Select(x => x.First().v)
                        .OrderByDescending(x => x.Similar.Count).ToList();
            }
        }

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };
        
        public async void Execute(CoroutineExecutionContext context)
        {
            /*this.results = new ConcurrentBag<ResultViewModel>();
            this.current = 0;
            this.CanRun = true;

            await this.ProgressViewModel.ShowProgress();
            await this.ProgressViewModel.UpdateProgress("[ 1 / 3 ] Starting analysis", 0, 1);

            IHashCalculator hashCalculator = null;
            IHashComparer hashComparer = null;
            if (this.Algorithm == "pHash")
            {
                hashCalculator = this.phash_simFinder;
                hashComparer = this.phash_simComparer;
            }
            else
            {
                hashCalculator = this.dhash_simFinder;
                hashComparer = this.dhash_simComparer;
            }

            hashCalculator.OnProgress += OnPrepProgress;
            hashComparer.OnCompareProgress += OnRunProgress;
            
            var allfiles = Directory.EnumerateFiles(Pathing.GetUNCPath(this.DirectoryName), "*.jpg", SearchOption.AllDirectories);
            this.total = allfiles.Count();
            await Task.Run(() => hashCalculator.Run(allfiles, () => this.CanRun));
            this.current = 0;
            await Task.Run(() => hashComparer.Run(allfiles, MaxDistance, () => this.CanRun));

            hashCalculator.OnProgress -= OnPrepProgress;
            hashComparer.OnCompareProgress -= OnRunProgress;

            await this.ProgressViewModel.CloseProgress();*/

            Completed(this, new ResultCompletionEventArgs());
        }

        private void OnPrepProgress()
        {
            current++;
            this.ProgressViewModel.UpdateProgress(string.Format("[ 2 / 3 ] Preparing: {0:0.00} % ({1} / {2})", (current * 1.0 / total) * 100.0, current, total), current, total);
        }

        private void OnRunProgress(string file, Distance[] files)
        {
            if (files != null)
            {
                var r = new ResultViewModel(this.eventAggregator);
                r.AddDistances(file, files);
                this.results.Add(r);
            }

            current++;
            this.ProgressViewModel.UpdateProgress(string.Format("[ 3 / 3 ] Comparing: {0:0.00} % ({1} / {2})", (current * 1.0 / total) * 100.0, current, total), current, total);
        }
    }
}
