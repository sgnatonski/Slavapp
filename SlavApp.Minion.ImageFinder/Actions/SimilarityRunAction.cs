using Caliburn.Micro;
using SlavApp.ImageFinder;
using SlavApp.ImageFinder.DHash;
using SlavApp.ImageFinder.PHash;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlavApp.Minion.ImageFinder.Actions
{
    public class SimilarityRunAction : IResult
    {
        private readonly DHashCalculator dhash_simFinder;
        private readonly DHashComparer dhash_simComparer;
        private readonly PHashCalculator phash_simFinder;
        private readonly PHashComparer phash_simComparer;
        public SimilarityRunAction(DHashCalculator dsimFinder, DHashComparer dsimComparer, PHashCalculator psimFinder, PHashComparer psimComparer)
        {
            this.dhash_simFinder = dsimFinder;
            this.dhash_simComparer = dsimComparer;
            this.phash_simFinder = psimFinder;
            this.phash_simComparer = psimComparer;
        }

        public string Algorithm { get; set; }
        public string DirectoryName { get; set; }
        public int MaxDistance { get; set; }
        public bool CanRun { get; set; }

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };
        public event EventHandler<SimilarityRunEventArgs> OnCompareProgress = delegate { };
        public event EventHandler<PrepareEventArgs> OnPrepareProgress = delegate { };

        private int total;

        public async void Execute(CoroutineExecutionContext context)
        {
            this.CanRun = true;
            var allfiles = System.IO.Directory.EnumerateFiles(Pathing.GetUNCPath(this.DirectoryName), "*.jpg", System.IO.SearchOption.AllDirectories);
            this.total = await Task.Run<int>(() => allfiles.Count());

            if (this.Algorithm == "pHash")
            {
                this.phash_simFinder.OnProgress += OnPrepProgress;
                this.phash_simComparer.OnCompareProgress += OnRunProgress;

                await Task.Run(() => this.phash_simFinder.Run(allfiles, () => this.CanRun));
                await Task.Run(() => this.phash_simComparer.Run(allfiles, MaxDistance, () => this.CanRun));

                this.phash_simFinder.OnProgress -= OnPrepProgress;
                this.phash_simComparer.OnCompareProgress -= OnRunProgress;
            }
            else
            {
                this.dhash_simFinder.OnProgress += OnPrepProgress;
                this.dhash_simComparer.OnCompareProgress += OnRunProgress;

                await Task.Run(() => this.dhash_simFinder.Run(allfiles, () => this.CanRun));
                await Task.Run(() => this.dhash_simComparer.Run(allfiles, MaxDistance, () => this.CanRun));

                this.dhash_simFinder.OnProgress -= OnPrepProgress;
                this.dhash_simComparer.OnCompareProgress -= OnRunProgress;
            }

            Completed(this, new ResultCompletionEventArgs());
        }

        private void OnPrepProgress()
        {
            OnPrepareProgress(this, new PrepareEventArgs(this.total));
        }

        private void OnRunProgress(Distance[] files)
        {
            OnCompareProgress(this, new SimilarityRunEventArgs(this.total, files));
        }
    }
}
