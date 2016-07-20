using DBreeze;
using DBreeze.Transactions;
using log4net;
using SlavApp.Resembler.Storage;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SlavApp.Resembler.DHash
{
    public class DHashCalculator : IHashCalculator
    {
        public event ProgressEventHandler OnProgress;

        private readonly DBreezeInstance dbInstance;
        private readonly HashRepository hashes;
        private readonly ILog log;

        public DHashCalculator(DBreezeInstance dbInstance, HashRepository hashes, ILog log)
        {
            this.dbInstance = dbInstance;
            this.hashes = hashes;
            this.log = log;
        }

        public void Run(IEnumerable<string> files)
        {
            this.Run(files, () => true, () => false);
        }

        public void Run(IEnumerable<string> files, Func<bool> continueTest, Func<bool> pauseTest)
        {
            files.AsParallel().ForAll(f =>
            {
                if (continueTest())
                {
                    while (pauseTest())
                    {
                        Thread.Sleep(1000);
                    }

                    using (var tran = dbInstance.GetTransaction())
                    {
                        GetHash(tran, Pathing.GetUNCPath(f));
                        tran.Commit();
                    }
                    OnProgress(f);
                }
            });
        }

        private ulong GetHash(Transaction tran, string filename)
        {
            var pcData = tran.Select<string, ulong>("dhash", filename);
            if (!pcData.Exists)
            {
                try
                {
                    var hash = new DHash().Compute(filename);
                    tran.Insert("dhash", filename, hash);
                    return hash;
                }
                catch (Exception ex)
                {
                    this.log.Warn("Hash calculation failed", ex);
                }
            }
            return pcData.Value;
        }
    }
}