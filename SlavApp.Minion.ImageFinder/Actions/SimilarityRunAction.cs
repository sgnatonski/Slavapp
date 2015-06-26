using Caliburn.Micro;
using ImageFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.ImageFinder.Actions
{
    public class SimilarityRunAction : IResult
    {
        private readonly SimilarFinder simFinder;
        public SimilarityRunAction(SimilarFinder simFinder)
        {
            this.simFinder = simFinder;
            this.simFinder.OnProgress += OnRunProgress;
        }

        public string DirectoryName { get; set; }
        public List<SimilarityResult> Results { get; private set; }

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };
        public event EventHandler<SimilarityRunEventArgs> OnProgress = delegate { };

        public async void Execute(CoroutineExecutionContext context)
        {
            this.Results = await Task.Run(() => this.simFinder.Run(this.DirectoryName, "*.jpg", 0.6));
            
            Completed(this, new ResultCompletionEventArgs());
        }

        private void OnRunProgress(int total, string file1, string file2, double value)
        {
            OnProgress(this, new SimilarityRunEventArgs(total, file1, file2, value));
        }
    }

    public class SimilarityRunEventArgs :EventArgs
    {
        public SimilarityRunEventArgs(int total, string file1, string file2, double value)
        {
            Total = total;
            File1 = file1;
            File2 = file2;
            Value = value;
        }
        public int Total { get; private set; }
        public string File1 { get; private set; }
        public string File2 { get; private set; }
        public double Value { get; private set; }
    }
}
