using DBreeze;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
            });

            this.isInitialized = true;
        }

        public void Run(string directory, string filter, double minSimilarity)
        {
            this.Run(directory, filter, minSimilarity, () => true);
        }

        public void Run(string directory, string filter, double minSimilarity, Func<bool> continueTest)
        {
            var maxSimilarityDistance = 20 - (int)(20 * (minSimilarity / 100.0));
            var allfiles = System.IO.Directory.GetFiles(directory, filter, System.IO.SearchOption.AllDirectories);

            Dictionary<string, ulong> dict = null;
            Dictionary<ulong, List<string>> hashes = null;
            using (var tMemory = this.memoryEngine.GetTransaction())
            {
                dict = tMemory.SelectForward<string, ulong>("hash").ToDictionary(x => x.Key, y => y.Value);
                hashes = dict.GroupBy(p => p.Value).ToDictionary(g => g.Key, g => g.Select(pp => pp.Key).ToList());
            }
            var hashesArray = hashes.Keys.ToArray();

            var tree = VPTree.Build(hashesArray, ph_hamming_distance);

            allfiles.AsParallel().ForAll(f =>
            {
                if (continueTest())
                {
                    var result = tree.searchVPTree(dict[f], 10, maxSimilarityDistance);
                    var files = result.Select(x => hashesArray[x.i]).Distinct().Select(x => hashes[x]).SelectMany(x => x).ToArray();

                    if (files.Any(x => x != f))
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
                tMemory.Commit();
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