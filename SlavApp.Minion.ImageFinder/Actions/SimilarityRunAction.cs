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

        public async void Execute(CoroutineExecutionContext context)
        {
            this.CanRun = true;
            var allfiles = System.IO.Directory.EnumerateFiles(Pathing.GetUNCPath(this.DirectoryName), "*.jpg", System.IO.SearchOption.AllDirectories);
            var filesCount = await Task.Run<int>(() => allfiles.Count());

            await Task.Run(() => this.simFinder.Run(allfiles, filesCount, () => this.CanRun));
            await Task.Run(() => this.simComparer.Run(allfiles, filesCount, MaxDistance, () => this.CanRun));

            Completed(this, new ResultCompletionEventArgs());
        }

        private void OnPrepProgress(long total)
        {
            OnPrepareProgress(this, new PrepareEventArgs(total));
        }

        private void OnRunProgress(long total, string file1, string[] file2, double value)
        {
            OnCompareProgress(this, new SimilarityRunEventArgs(total, file1, file2, value));
        }
    }

    public class SimilarityRunEventArgs :EventArgs
    {
        public SimilarityRunEventArgs(long total, string file1, string[] file2, double value)
        {
            Total = total;
            File1 = file1;
            File2 = file2;
            Value = value;
        }
        public long Total { get; private set; }
        public string File1 { get; private set; }
        public string[] File2 { get; private set; }
        public double Value { get; private set; }
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
