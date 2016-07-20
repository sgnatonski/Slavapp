using Caliburn.Micro;
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
    public class GetHashesAction : IResult
    {
        private readonly HashRepository hashRepository;

        public GetHashesAction(HashRepository hashRepository)
        {
            this.hashRepository = hashRepository;
        }

        public int HashesCount { get; private set; }
        
        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };
        public void Execute(CoroutineExecutionContext context)
        {
            var hashes = hashRepository.GetHashedFiles();
            this.HashesCount = hashes.Count();
            Completed(this, new ResultCompletionEventArgs());
        }
    }
}
