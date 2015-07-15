using DBreeze;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SlavApp.ImageFinder
{
    public class DHashComparer
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

        public void Run(IEnumerable<string> files, int filesCount, int distance)
        {
            this.Run(files, filesCount, distance, () => true);
        }

        public void Run(IEnumerable<string> files, int filesCount, int distance, Func<bool> continueTest)
        {
            Dictionary<string, ulong> dict = null;
            Dictionary<ulong, List<string>> hashes = null;
            using (var engine = new DBreezeEngine(dbConf))
            using (var tran = engine.GetTransaction())
            {
                dict = tran.SelectForward<string, ulong>("dhash").ToDictionary(x => x.Key, y => y.Value);
                hashes = dict.Flip();
            }
            var hashesArray = hashes.Keys.ToArray();

            var tree = VPTree.Build(hashesArray, ph_hamming_distance);

            files.AsParallel().ForAll(f =>
            {
                if (continueTest())
                {
                    ulong hash = 0;
                    if (dict.TryGetValue(f, out hash))
                    {
                        var result = tree.searchVPTree(hash, 10, distance);
                        var similarFiles = result.Select(x => hashesArray[x.i]).Distinct().Select(x => hashes[x]).SelectMany(x => x).Select(x => Pathing.GetUNCPath(x)).Distinct().ToArray();

                        if (similarFiles.Any(x => x != f))
                        {
                            OnCompareProgress(filesCount, f, similarFiles, 0);
                        }
                        else
                        {
                            OnCompareProgress(filesCount, null, null, 0);
                        }
                    }
                    else
                    {
                        // todo:
                    }
                }
            });
        }
    }
}