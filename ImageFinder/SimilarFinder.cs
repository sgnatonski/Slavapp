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
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject;
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        public delegate void CompareProgressEventHandler(long total, string file1, string file2, double value);

        public event CompareProgressEventHandler OnCompareProgress;

        public delegate void PrepareProgressEventHandler(long total);

        public event PrepareProgressEventHandler OnPrepareProgress;

        public void Run(string directory, string filter, double minSimilarity)
        {
            this.Run(directory, filter, minSimilarity, () => true);
        }

        public void Run(string directory, string filter, double minSimilarity, Func<bool> continueTest)
        {
            //Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            var allfiles = System.IO.Directory.GetFiles(directory, filter, System.IO.SearchOption.AllDirectories);

            using (var engine = new DBreezeEngine(dbConf))
            {
                this.memoryEngine = new DBreezeEngine(memConf);

                LoadFromDb(engine);

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

                if (continueTest())
                {
                    var list = ((ushort)allfiles.Length).GetIndexCombinations();
                    var total = list.Count();

                    var rangePartitioner = Partitioner.Create(0, total);

                    Parallel.ForEach(rangePartitioner, range =>
                    {
                        Debug.WriteLine("Starting range {0}-{1}", range.Item1, range.Item2);
                        using (var tran = this.memoryEngine.GetTransaction())
                        {
                            tran.SynchronizeTables("comp");
                            for (int i = range.Item1; i < range.Item2; i++)
                            {
                                if (!continueTest())
                                {
                                    break;
                                }
                                var c = list.ElementAt(i);
                                var result = GetComparisonResult(tran, allfiles.ElementAt(c.First()), allfiles.ElementAt(c.Last()), total, minSimilarity);
                            }
                            tran.Commit();
                        }
                    });
                }

                SaveToDb(engine);

                this.memoryEngine.Dispose();
            }
        }

        private SimilarityResult GetComparisonResult(Transaction tran, string first, string second, long total, double minValue)
        {
            var value = 0.0;
            var key = string.Join(".", new[] { first, second }.OrderBy(x => x));

            var compData = tran.Select<string, double>("comp", key);
            if (!compData.Exists)
            {
                value = this.GetHistogramComparison(tran, first, second, value);
                tran.Insert("comp", key, value);
                Debug.Write(".");
            }
            else
            {
                value = compData.Value;
            }

            if (OnCompareProgress != null)
            {
                if (value >= minValue)
                {
                    OnCompareProgress(total, first, second, value);
                }
                else
                {
                    OnCompareProgress(total, null, null, 0);
                }
            }

            return new SimilarityResult { First = first, Second = second, Value = value };
        }

        private double GetHistogramComparison(Transaction tran, string first, string second, double value)
        {
            var pc = GetHistogram(tran, first);
            var cc = GetHistogram(tran, second);

            value = pc.CalculateSimilarity(cc);
            return value;
        }

        private static ComparableImage GetHistogram(Transaction tran, string first)
        {
            ComparableImage pc = null;
            var pcData = tran.Select<string, DbMJSON<double[][]>>("hist", first);
            if (!pcData.Exists)
            {
                pc = new ComparableImage(new FileInfo(first));
                tran.Insert("hist", first, new DbMJSON<double[][]>(pc.ToArray()));
                Debug.Write("+");
            }
            else
            {
                pc = new ComparableImage(new FileInfo(first), pcData.Value.Get);
            }
            return pc;
        }

        private void LoadFromDb(DBreezeEngine engine)
        {
            using (var tran = engine.GetTransaction())
            using (var tMemory = this.memoryEngine.GetTransaction())
            {
                foreach (var row in tran.SelectForward<string, DbMJSON<double[][]>>("hist"))
                {
                    tMemory.Insert("hist", row.Key, row.Value);
                }
                foreach (var row in tran.SelectForward<string, double>("comp"))
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
                foreach (var row in tMemory.SelectForward<string, DbMJSON<double[][]>>("hist"))
                {
                    tran.Insert("hist", row.Key, row.Value);
                }
                tran.Commit(); 
                tran.RemoveAllKeys("comp", true);
                foreach (var row in tMemory.SelectForward<string, double>("comp"))
                {
                    tran.Insert("comp", row.Key, row.Value);
                }
                tran.Commit();
            }
        }
    }
}
