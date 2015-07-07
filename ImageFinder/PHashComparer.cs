using EyeOpen.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBreeze;
using DBreeze.Transactions;
using DBreeze.DataTypes;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Jint;

namespace ImageFinder
{
    public class PHashComparer : IDisposable
    {
        [DllImport(@"pHash.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ph_hamming_distance(ulong hasha, ulong hashb);

        public delegate void CompareProgressEventHandler(long total, string file1, string[] file2, double value);

        public event CompareProgressEventHandler OnCompareProgress;

        private readonly DBreezeConfiguration dbConf = new DBreezeConfiguration()
        {
            DBreezeDataFolderName = Path.Combine(Utils.GetAssemblyPath(), "DBR"),
            Storage = DBreezeConfiguration.eStorage.DISK
        };
        private readonly DBreezeConfiguration memConf = new DBreezeConfiguration()
        {
            Storage = DBreezeConfiguration.eStorage.MEMORY
        };

        private readonly Engine _engine = new Engine(); 
        private DBreezeEngine memoryEngine;
        private bool isInitialized = false;
        static PHashComparer()
        {
            DBreeze.Utils.CustomSerializator.ByteArraySerializator = ListExtenstions.SerializeProtobuf;
            DBreeze.Utils.CustomSerializator.ByteArrayDeSerializator = ListExtenstions.DeserializeProtobuf;
        }

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

                var sb = new StringBuilder();
                var vptree = File.ReadAllText(Path.Combine(Utils.GetAssemblyPath(), "js/vptree.js"));
                var main = File.ReadAllText(Path.Combine(Utils.GetAssemblyPath(), "js/main.js"));
                sb.Append(vptree);
                sb.Append(Environment.NewLine);
                sb.Append(main);

                _engine.SetValue("hamming_distance", new Func<ulong, ulong, int>((x, y) =>
                {
                    return ph_hamming_distance(x, y);
                }));
                _engine.Execute(sb.ToString());
            });

            this.isInitialized = true;
        }

        public void Run(string directory, string filter, double minSimilarity)
        {
            this.Run(directory, filter, minSimilarity, () => true);
        }

        public class VPTreeResult
        {
            public int i { get; set; }
            public int d { get; set; }
        }

        public void Run(string directory, string filter, double minSimilarity, Func<bool> continueTest)
        {
            var maxSimilarityDistance = 20 - (int)(20 * (minSimilarity / 100.0));
            var allfiles = System.IO.Directory.GetFiles(directory, filter, System.IO.SearchOption.AllDirectories);

            Dictionary<ulong, List<string>> hashes = null;
            using (var tMemory = this.memoryEngine.GetTransaction())
            {
                var dict = tMemory.SelectForward<string, ulong>("hash").ToDictionary(x => x.Key, y => y.Value);
                hashes = dict.GroupBy(p => p.Value).ToDictionary(g => g.Key, g => g.Select(pp => pp.Key).ToList());
            }
            var hashesArray = hashes.Keys.ToArray();

            _engine.Invoke("build", hashesArray);

            allfiles.ToList().ForEach(f =>
            {
                if (continueTest())
                {
                    ulong hash = 0;
                    using (var tMemory = this.memoryEngine.GetTransaction())
                    {
                        hash = tMemory.Select<string, ulong>("hash", f).Value;
                    }
                    var result = _engine.Invoke("search", hash, 10, maxSimilarityDistance);
                    var a = result.AsArray();
                    var l = (int)a.Get("length").AsNumber();
                    var values = Enumerable.Range(0, l).Select(x => a.GetProperty(x.ToString()).Value.Value.AsObject().Properties.Select(p => new { Key = p.Key, Value = p.Value.Value.Value.AsNumber() }).ToList()).ToList();
                    var foundHashes = values.Select(x => x.Where(p => p.Key == "i").Select(p => hashesArray[(int)p.Value])).SelectMany(x => x).Distinct();
                    var files = foundHashes.Select(x => hashes[x].Where(c => !c.Contains(f)).Select(c => c)).SelectMany(x => x).ToArray();
                    if (files.Any())
                    {
                        OnCompareProgress(allfiles.Count(), f, files, 0);
                    }
                    else
                    {
                        OnCompareProgress(allfiles.Count(), null, null, 0);
                    }
                }
            });
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
                //tMemory.Commit();
                /*foreach (var row in tran.SelectForward<long, int>("dist"))
                {
                    tMemory.Insert("dist", row.Key, row.Value);
                }*/
                tMemory.Commit();
            }
        }

        private void SaveToDb(DBreezeEngine engine)
        {
            /*using (var tran = engine.GetTransaction())
            using (var tMemory = this.memoryEngine.GetTransaction())
            {
                tran.RemoveAllKeys("dist", true);
                foreach (var row in tMemory.SelectForward<long, int>("dist"))
                {
                    tran.Insert("dist", row.Key, row.Value);
                }
                tran.Commit();
            }*/
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
