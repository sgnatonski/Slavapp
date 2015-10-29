using DBreeze;
using DBreeze.Transactions;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SlavApp.ImageFinder.PHash
{
    public class PHashCalculator
    {
        [DllImport(@"pHash.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ph_dct_imagehash(string file, ref ulong hash);

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
            var pcData = tran.Select<string, ulong>("hash", filename);
            if (!pcData.Exists)
            {
                try
                {
                    ulong hash = 0;
                    var sw = Stopwatch.StartNew();
                    ph_dct_imagehash(filename, ref hash);
                    Debug.WriteLine(string.Format("ph_dct_imagehash: {0} ms", sw.ElapsedMilliseconds));
                    tran.Insert("hash", filename, hash);
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