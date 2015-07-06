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
using System.Numerics;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace ImageFinder
{
    public class SimilarPHashFinder
    {
        [DllImport(@"pHash.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ph_dct_imagehash(string file, ref ulong hash);
        [DllImport(@"pHash.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ph_hamming_distance(ulong hasha, ulong hashb);

        public delegate void CompareProgressEventHandler(long total, string file1, string file2, double value);

        public event CompareProgressEventHandler OnCompareProgress;

        public delegate void PrepareProgressEventHandler(long total);

        public event PrepareProgressEventHandler OnPrepareProgress;

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
        static SimilarPHashFinder()
        {
            DBreeze.Utils.CustomSerializator.ByteArraySerializator = ListExtenstions.SerializeProtobuf;
            DBreeze.Utils.CustomSerializator.ByteArrayDeSerializator = ListExtenstions.DeserializeProtobuf;
        }

        public async Task<bool> Initialize()
        {
            return await Task.Run(() =>
            {
                this.memoryEngine = new DBreezeEngine(memConf);

                using (var engine = new DBreezeEngine(dbConf))
                {
                    LoadFromDb(engine);
                };

                return true;
            });
        }

        public void Run(string directory, string filter, double minSimilarity)
        {
            this.Run(directory, filter, minSimilarity, () => true);
        }

        public void Run(string directory, string filter, double minSimilarity, Func<bool> continueTest)
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
                        OnPrepareProgress(allfiles.Length);
                    }
                });

                var list = ((ushort)allfiles.Length).GetIndexCombinations();
                var total = list.Count();

                if (continueTest())
                {
                    try
                    {
                        list.AsParallel().ForAll(c =>
                        {
                            if (!continueTest())
                            {
                                return;
                            }
                            var f = allfiles[c[0]];
                            var l = allfiles[c[1]];
                            var value = 0.0;
                            using (var tran = this.memoryEngine.GetTransaction())
                            {
                                tran.ValuesLazyLoadingIsOn = false;
                                value = GetComparisonResult(tran, f, l);
                                tran.Commit();
                            }
                            if (OnCompareProgress != null)
                            {
                                if (100 - value >= minSimilarity)
                                {
                                    OnCompareProgress(total, f, l, value);
                                }
                                else
                                {
                                    OnCompareProgress(total, null, null, 0);
                                }
                            }
                        });
                    }
                    finally
                    {
                        SaveToDb(engine);
                    }
                }
                else
                {
                    SaveToDb(engine);
                }
            }
        }

        private double GetComparisonResult(Transaction tran, string first, string second)
        {
            var value = 0;
            var key = string.Join("|~|", new[] { first, second }.OrderBy(x => x)).GetInt64HashCode();

            var compData = tran.Select<long, int>("dist", key);

            if (!compData.Exists)
            {
                var pc = GetHash(tran, first);
                var cc = GetHash(tran, second);
                value = ph_hamming_distance(pc, cc);
                tran.Insert("dist", key, value);
            }
            else
            {
                value = compData.Value;
            }

            return value;
        }

        private static ulong GetHash(Transaction tran, string first)
        {
            var pcData = tran.Select<string, ulong>("hash", first);
            if (!pcData.Exists)
            {
                ulong hash = 0;
                ph_dct_imagehash(first, ref hash);
                tran.Insert("hash", first, hash);
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
                foreach (var row in tran.SelectForward<long, int>("dist"))
                {
                    tMemory.Insert("dist", row.Key, row.Value);
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
                tran.RemoveAllKeys("dist", true);
                foreach (var row in tMemory.SelectForward<long, int>("dist"))
                {
                    tran.Insert("dist", row.Key, row.Value);
                }
                tran.Commit();
            }
        }
    }
}
