using DBreeze;
using SlavApp.Resembler.Storage;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SlavApp.Resembler
{
    public class DHashComparer : IHashComparer
    {
        public event CompareProgressEventHandler OnCompareProgress;

        private readonly DBreezeInstance dbInstance;

        public DHashComparer(DBreezeInstance dbInstance)
        {
            this.dbInstance = dbInstance;
        }

        public void Run(IEnumerable<string> files, int distance)
        {
            this.Run(files, distance, () => true);
        }

        public void Run(IEnumerable<string> files, int distance, Func<bool> continueTest)
        {
            Dictionary<string, ulong> dict = null;
            Dictionary<ulong, List<string>> hashes = null;
            using (var tran = this.dbInstance.GetTransaction())
            {
                dict = tran.SelectForward<string, ulong>("dhash").ToDictionary(x => x.Key, y => y.Value);
                hashes = dict.Flip();
            }
            var hashesArray = hashes.Keys.ToArray();

            var tree = VPTree.Build(hashesArray, HammingDistance.GetDistance);

            files.AsParallel().ForAll(f =>
            {
                if (continueTest())
                {
                    ulong hash = 0;
                    if (dict.TryGetValue(f, out hash))
                    {
                        var result = tree.SearchVPTree(hash, 20, distance + 1);
                        var similarFiles = result.Select(x => hashes[hashesArray[x.i]].Select(y => new Distance
                        {
                            DistanceBetween = x.d,
                            Filename1 = f,
                            Filename2 = y,
                            Hash1 = hash,
                            Hash2 = hashesArray[x.i]
                        })).SelectMany(x => x).Where(x => x.Filename2 != f && File.Exists(x.Filename2)).Distinct().ToArray();

                        if (similarFiles.Any())
                        {
                            OnCompareProgress(f, similarFiles);
                        }
                        else
                        {
                            OnCompareProgress(f, null);
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