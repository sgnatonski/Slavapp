using DBreeze;
using DBreeze.Transactions;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SlavApp.ImageFinder.DHash
{
    public class DHashCalculator
    {
        public delegate void ProgressEventHandler(long total);

        public event ProgressEventHandler OnProgress;

        private readonly DBreezeConfiguration dbConf = new DBreezeConfiguration()
        {
            DBreezeDataFolderName = Path.Combine(Utils.GetAssemblyPath(), "DBR"),
            Storage = DBreezeConfiguration.eStorage.DISK
        };

        public void Run(IEnumerable<string> files, int filesCount)
        {
            this.Run(files, filesCount, () => true);
        }

        public void Run(IEnumerable<string> files, int filesCount, Func<bool> continueTest)
        {
            using (var engine = new DBreezeEngine(dbConf))
            {
                files.AsParallel().ForAll(f =>
                {
                    if (continueTest())
                    {
                        using (var tran = engine.GetTransaction())
                        {
                            GetHash(tran, Pathing.GetUNCPath(f));
                            tran.Commit();
                        }
                        OnProgress(filesCount);
                    }
                });
            }
        }

        private static ulong GetHash(Transaction tran, string filename)
        {
            var pcData = tran.Select<string, ulong>("dhash", filename);
            if (!pcData.Exists)
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    var hash = new DHash().Compute(filename);
                    Debug.WriteLine(string.Format("DHash().Compute: {0} ms", sw.ElapsedMilliseconds));
                    tran.Insert("dhash", filename, hash);
                    return hash;
                }
                catch (Exception)
                {
                }
            }
            return pcData.Value;
        }
    }
}