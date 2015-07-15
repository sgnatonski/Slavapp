using DBreeze;
using DBreeze.Transactions;
using SlavApp.Windows;
using System;
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

        public void Run(string directory, string filter)
        {
            this.Run(directory, filter, () => true);
        }

        public void Run(string directory, string filter, Func<bool> continueTest)
        {
            var allfiles = System.IO.Directory.GetFiles(Pathing.GetUNCPath(directory), filter, System.IO.SearchOption.AllDirectories);

            using (var engine = new DBreezeEngine(dbConf))
            {
                allfiles.AsParallel().ForAll(f =>
                {
                    if (continueTest())
                    {
                        using (var tran = engine.GetTransaction())
                        {
                            GetHash(tran, Pathing.GetUNCPath(f));
                            tran.Commit();
                        }
                        OnProgress(allfiles.Length);
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