using DBreeze;
using DBreeze.Transactions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImageFinder
{
    public class PHashCalculator : IDisposable
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

        private readonly DBreezeConfiguration memConf = new DBreezeConfiguration()
        {
            Storage = DBreezeConfiguration.eStorage.MEMORY
        };

        private DBreezeEngine memoryEngine;
        private bool isInitialized;

        public async Task Initialize()
        {
            if (this.isInitialized)
            {
                return;
            }
            await Task.Run(() =>
            {
                this.memoryEngine = new DBreezeEngine(memConf);

                using (var engine = new DBreezeEngine(dbConf))
                {
                    LoadFromDb(engine);
                };
            });
            this.isInitialized = true;
        }

        public void Run(string directory, string filter)
        {
            this.Run(directory, filter, () => true);
        }

        public void Run(string directory, string filter, Func<bool> continueTest)
        {
            var allfiles = System.IO.Directory.GetFiles(directory, filter, System.IO.SearchOption.AllDirectories);

            using (var engine = new DBreezeEngine(dbConf))
            {
                allfiles.AsParallel().ForAll(f =>
                {
                    if (continueTest())
                    {
                        using (var tran = this.memoryEngine.GetTransaction())
                        {
                            GetHash(tran, f);
                            tran.Commit();
                        }
                        OnProgress(allfiles.Length);
                    }
                });

                SaveToDb(engine);
            }
        }

        private static ulong GetHash(Transaction tran, string filename)
        {
            var pcData = tran.Select<string, ulong>("hash", filename);
            if (!pcData.Exists)
            {
                ulong hash = 0;
                var sw = Stopwatch.StartNew();
                ph_dct_imagehash(filename, ref hash);
                Debug.WriteLine(string.Format("ph_dct_imagehash: {0} ms", sw.ElapsedMilliseconds));
                tran.Insert("hash", filename, hash);
                return hash;
            }
            return pcData.Value;
        }

        private void LoadFromDb(DBreezeEngine engine)
        {
            using (var tran = engine.GetTransaction())
            using (var tMemory = this.memoryEngine.GetTransaction())
            {
                foreach (var row in tran.SelectForward<string, ulong>("hash"))
                {
                    tMemory.Insert("hash", row.Key, row.Value);
                }
                tMemory.Commit();
            }
        }

        private void SaveToDb(DBreezeEngine engine)
        {
            using (var tran = engine.GetTransaction())
            using (var tMemory = this.memoryEngine.GetTransaction())
            {
                tran.RemoveAllKeys("hash", true);
                foreach (var row in tMemory.SelectForward<string, ulong>("hash"))
                {
                    tran.Insert("hash", row.Key, row.Value);
                }
                tran.Commit();
            }
        }

        public void Dispose()
        {
            if (this.memoryEngine != null)
            {
                this.memoryEngine.Dispose();
            }
        }
    }
}