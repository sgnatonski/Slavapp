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

namespace ImageFinder
{
    public class SimilarFinder
    {
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
        public SimilarFinder()
        {
            DBreeze.Utils.CustomSerializator.ByteArraySerializator = ListExtenstions.SerializeProtobuf;
            DBreeze.Utils.CustomSerializator.ByteArrayDeSerializator = ListExtenstions.DeserializeProtobuf;
        }

        public delegate void CompareProgressEventHandler(long total, string file1, string file2, double value);

        public event CompareProgressEventHandler OnCompareProgress;

        public delegate void PrepareProgressEventHandler(long total);

        public event PrepareProgressEventHandler OnPrepareProgress;

        public event EventHandler OnInitialized;

        public void Initialize()
        {
            Task.Run(() =>
            {
                this.memoryEngine = new DBreezeEngine(memConf);

                using (var engine = new DBreezeEngine(dbConf))
                {
                    LoadFromDb(engine);
                };

                if (OnInitialized != null)
                {
                    OnInitialized(this, new EventArgs());
                }
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
                            GetHistogram(tran, f);
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
                            using (var tran = this.memoryEngine.GetTransaction())
                            {
                                //tran.SynchronizeTables("comp");
                                tran.ValuesLazyLoadingIsOn = false;
                                if (!continueTest())
                                {
                                    return;
                                }
                                var f = allfiles[c[0]];
                                var l = allfiles[c[1]];
                                var value = GetComparisonResult(tran, f, l);
                                if (OnCompareProgress != null)
                                {
                                    if (value >= minSimilarity)
                                    {
                                        OnCompareProgress(total, f, l, value);
                                    }
                                    else
                                    {
                                        OnCompareProgress(total, null, null, 0);
                                    }
                                }
                                tran.Commit();
                            }
                        });
                    }
                    finally
                    {
                        SaveToDb(engine);
                    }
                }
            }
        }

        private double GetComparisonResult(Transaction tran, string first, string second)
        {
            var value = 0.0;
            var key = string.Join("|~|", new[] { first, second }.OrderBy(x => x)).GetInt64HashCode();

            var compData = tran.Select<long, double>("comp", key);

            if (!compData.Exists)
            {
                var pc = GetHistogram(tran, first);
                var cc = GetHistogram(tran, second);
                value = pc.CalculateSimilarity(cc);
                tran.Insert("comp", key, value);
            }
            else
            {
                value = compData.Value;
            }

            return value;
        }

        private static ComparableImage GetHistogram(Transaction tran, string first)
        {
            ComparableImage pc = null;
            var pcData = tran.Select<string, HistogramData>("hist", first);
            if (!pcData.Exists)
            {
                pc = new ComparableImage(new FileInfo(first));
                tran.Insert("hist", first, new HistogramData(pc.Projections.HorizontalProjection,pc.Projections.VerticalProjection));
            }
            else
            {
                pc = new ComparableImage(new FileInfo(first), pcData.Value.X, pcData.Value.Y);
            }
            return pc;
        }

        private void LoadFromDb(DBreezeEngine engine)
        {
            using (var tran = engine.GetTransaction())
            using (var tMemory = this.memoryEngine.GetTransaction())
            {
                foreach (var row in tran.SelectForward<string, HistogramData>("hist"))
                {
                    tMemory.Insert("hist", row.Key, row.Value);
                }
                foreach (var row in tran.SelectForward<long, double>("comp"))
                {
                    tMemory.Insert("comp", row.Key, row.Value);
                }
                tMemory.Commit();
            }
        }

        private void SaveToDb(DBreezeEngine engine)
        {
            using (var tran = engine.GetTransaction())
            using (var tMemory = this.memoryEngine.GetTransaction())
            {
                tran.RemoveAllKeys("hist", true);
                foreach (var row in tMemory.SelectForward<string, HistogramData>("hist"))
                {
                    tran.Insert("hist", row.Key, row.Value);
                }
                tran.Commit(); 
                tran.RemoveAllKeys("comp", true);
                foreach (var row in tMemory.SelectForward<long, double>("comp"))
                {
                    tran.Insert("comp", row.Key, row.Value);
                }
                tran.Commit();
            }
        }
    }
}
