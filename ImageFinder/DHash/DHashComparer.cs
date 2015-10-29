using DBreeze;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SlavApp.ImageFinder
{
    public class DHashComparer
    {
        [DllImport(@"pHash.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ph_hamming_distance(ulong hasha, ulong hashb);

        public delegate void CompareProgressEventHandler(Distance[] file2);

        public event CompareProgressEventHandler OnCompareProgress;

        private readonly DBreezeConfiguration dbConf = new DBreezeConfiguration()
        {
            DBreezeDataFolderName = Path.Combine(Utils.GetAssemblyPath(), "DBR"),
            Storage = DBreezeConfiguration.eStorage.DISK
        };

        public void Run(IEnumerable<string> files, int distance)
        {
            this.Run(files, distance, () => true);
        }

        public void Run(IEnumerable<string> files, int distance, Func<bool> continueTest)
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
                        var result = tree.searchVPTree(hash, 10, distance + 1);
                        var similarFiles = result.Select(x => hashes[hashesArray[x.i]].Select(y => new Distance { DistanceBetween = x.d, Filename1 = f, Filename2 = Pathing.GetUNCPath(y) })).SelectMany(x => x).Distinct().ToArray();

                        if (similarFiles.Any(x => x.Filename2 != f))
                        {
                            OnCompareProgress(similarFiles);
                        }
                        else
                        {
                            OnCompareProgress(null);
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