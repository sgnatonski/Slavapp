using Caliburn.Micro;
using SlavApp.Minion.Plugin;
using SlavApp.Minion.Resembler.ViewModels;
using SlavApp.Resembler;
using SlavApp.Resembler.DHash;
using SlavApp.Resembler.PHash;
using SlavApp.Resembler.Storage;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlavApp.Minion.Resembler.Actions
{
    public class FindCopiesAction : IResult
    {
        private readonly MurmurRepository hashRepository;

        public FindCopiesAction(MurmurRepository hashRepository)
        {
            this.hashRepository = hashRepository;
        }

        public List<DirectoryCopyResultModel> CopyDirectories { get; private set; }

        public IProgressViewModel ProgressViewModel { get; set; }

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        public async void Execute(CoroutineExecutionContext context)
        {
            await this.ProgressViewModel.ShowProgress();

            await Task.Run(() =>
            {
                var hashes = hashRepository.GetMurMurs();
                var dirs = hashes.GroupBy(x => Path.GetDirectoryName(x.Key).ToLower())
                    .Select(x => new { Dir = x.Key, Files = x.Select(f => f.Value).ToList() })
                    .ToList()
                    .AsParallel();

                CopyDirectories = dirs.SelectMany((x, i) => dirs.Skip(i + 1).Select(y => new DirectoryCopyResultModel
                {
                    Directory1 = x.Dir,
                    Directory2 = y.Dir,
                    Directory1Files = x.Files.Count,
                    Directory2Files = y.Files.Count,
                    CopyCount = Math.Max(x.Files.Count, y.Files.Count) - DiffCount(x.Files, y.Files)
                }))
                .Where(x => x.CopyCount > 0 && x.CopyPercentage >= 0.5)
                .OrderByDescending(x => x.CopyPercentage)
                .ThenByDescending(x => x.Directory1Files)
                .ToList();
            });

            await this.ProgressViewModel.CloseProgress();

            Completed(this, new ResultCompletionEventArgs());
        }

        private int DiffCount(IEnumerable<string> f1, IEnumerable<string> f2)
        {
            return f1.Union(f2).Except(f1.Intersect(f2)).Count();
        }
    }
}
