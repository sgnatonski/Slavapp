using log4net;
using SlavApp.Resembler.Storage;
using SlavApp.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlavApp.Resembler.PHash
{
    public class PHashComparer : IHashComparer
    {        
        public event CompareProgressEventHandler OnCompareProgress;

        private readonly DBreezeInstance dbInstance;
        private readonly HashRepository hashes;
        private readonly MurmurRepository murmurs;
        private readonly ILog log;

        public PHashComparer(DBreezeInstance dbInstance, HashRepository hashes, MurmurRepository murmurs, ILog log)
        {
            this.dbInstance = dbInstance;
            this.hashes = hashes;
            this.murmurs = murmurs;
            this.log = log;
        }

        public void Run(IEnumerable<string> files, int distance)
        {
            this.Run(files, distance, () => true);
        }

        public void Run(IEnumerable<string> files, int distance, Func<bool> continueTest)
        {
            Dictionary<string, ulong> dict = null;
            Dictionary<ulong, List<string>> hashesCollection = null;
            using (var tran = this.dbInstance.GetTransaction())
            {
                var mm = murmurs.GetMurMurs();
                var hh = hashes.GetHashes();
                dict = mm.Where(x => hh.ContainsKey(x.Value))
                    .ToDictionary(x => x.Key, y => hh[y.Value]);
                hashesCollection = dict.Flip();
            }
            var hashesArray = hashesCollection.Keys.ToArray();

            var tree = VPTree.Build(hashesArray, HammingDistance.GetDistance);
            
            files/*.ToList().ForEach(f =>*/.AsParallel().ForAll(f =>
            {
                var file = Pathing.GetUNCPath(f);
                if (continueTest())
                {
                    ulong hash = 0;
                    if (dict.TryGetValue(file, out hash))
                    {
                        var result = tree.SearchVPTree(hash, 20, distance + 1);
                        var similarFiles = result.Select(x => hashesCollection[hashesArray[x.i]].Select(y => new Distance {
                            DistanceBetween = x.d,
                            Filename1 = file,
                            Filename2 = y,
                            Hash1 = hash,
                            Hash2 = hashesArray[x.i]
                        })).SelectMany(x => x).Where(x => x.Filename2 != file && File.Exists(x.Filename2)).Distinct().ToArray();

                        if (similarFiles.Any())
                        {
                            OnCompareProgress(file, similarFiles);
                        }
                        else
                        {
                            OnCompareProgress(file, null);
                        }
                    }
                    else
                    {
                        // todo:
                        System.Diagnostics.Debug.WriteLine(file);
                    }
                }
            });
        }
    }
}