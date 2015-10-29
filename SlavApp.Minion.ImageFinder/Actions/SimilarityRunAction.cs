using Caliburn.Micro;
using SlavApp.ImageFinder;
using SlavApp.ImageFinder.DHash;
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
        private readonly DHashCalculator simFinder;
        private readonly DHashComparer simComparer;
        public SimilarityRunAction(DHashCalculator simFinder, DHashComparer simComparer)
        {
            this.simFinder = simFinder;
            this.simFinder.OnProgress += OnPrepProgress;
            this.simComparer = simComparer;
            this.simComparer.OnCompareProgress += OnRunProgress;
        }

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

            await Task.Run(() => this.simFinder.Run(allfiles, () => this.CanRun));
            await Task.Run(() => this.simComparer.Run(allfiles, MaxDistance, () => this.CanRun));

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

    public class SimilarityRunEventArgs :EventArgs
    {
        public SimilarityRunEventArgs(long total, Distance[] files)
        {
            Total = total;
            Files = files;
        }
        public long Total { get; private set; }
        public Distance[] Files { get; private set; }
    }

    public class PrepareEventArgs : EventArgs
    {
        public PrepareEventArgs(long total)
        {
            Total = total;
        }
        public long Total { get; private set; }
    }
}
