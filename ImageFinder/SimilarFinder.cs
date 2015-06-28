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

namespace ImageFinder
{
    public class SimilarFinder
    {
        public delegate void ProgressEventHandler(int total, string file1, string file2, double value);

        public event ProgressEventHandler OnProgress;

        public List<SimilarityResult> Run(string directory, string filter, double minSimilarity)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            var allfiles = System.IO.Directory.GetFiles(directory, filter, System.IO.SearchOption.AllDirectories);

            var list = allfiles.GetPermutations(2);

            using (var engine = new DBreezeEngine(new DBreezeConfiguration()
            {
                DBreezeDataFolderName = @".\DBR1",
                Storage = DBreezeConfiguration.eStorage.DISK
            }))
            {
                DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject;
                DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;

                var total = list.Count();
                var results = list.AsParallel().Select(comp => GetComparisonResult(engine, comp, total, minSimilarity)).Where(x => x.Value >= minSimilarity).ToList();
                return results;
            }
        }

        private SimilarityResult GetComparisonResult(DBreezeEngine engine, IEnumerable<string> comp, int total, double minValue)
        {
            using (var tran = engine.GetTransaction())
            {
                tran.SynchronizeTables("hist");

                var f = comp.First();
                var s = comp.Last();
                ComparableImage pc = null;
                var pcData = tran.Select<string, DbMJSON<double[][]>>("hist", f);
                if (!pcData.Exists)
                {
                    pc = new ComparableImage(new FileInfo(f));
                    tran.Insert("hist", f, new DbMJSON<double[][]>(pc.ToArray()));
                    Debug.Write("+");
                }
                else
                {
                    pc = new ComparableImage(new FileInfo(f), pcData.Value.Get);
                }

                ComparableImage cc = null;
                var ccData = tran.Select<string, DbMJSON<double[][]>>("hist", s);
                if (!ccData.Exists)
                {
                    cc = new ComparableImage(new FileInfo(s));
                    tran.Insert("hist", s, new DbMJSON<double[][]>(cc.ToArray()));
                    Debug.Write("+");
                }
                else
                {
                    cc = new ComparableImage(new FileInfo(s), ccData.Value.Get);
                }

                tran.Commit();
                /*if (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 > 100 * 1024 * 1024)
                {
                    GC.Collect();
                    Debug.WriteLine("Gc.Collect {0}", System.Diagnostics.Process.GetCurrentProcess().WorkingSet64);
                }*/

                var value = pc.CalculateSimilarity(cc);
                if (OnProgress != null)
                {
                    if (value >= minValue)
                    {
                        OnProgress(total, f, s, value);
                    }
                    else
                    {
                        OnProgress(total, null, null, 0);
                    }
                }

                return new SimilarityResult { First = f, Second = s, Value = value };
            }
        }
    }

    public static class ListExtenstions
    {
        public static IEnumerable<T[]> GetPermutations<T>(this T[] list, int length)
        {
            if (length == 1)
            {
                return list.Select(t => new T[] { t });
            }

            return list.GetPermutations(length - 1).SelectMany(t => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }).ToArray());
        }

        public static IEnumerable<T> Every<T>(this IEnumerable<T> source, int count, Action<T> action)
        {
            int cnt = 0;
            foreach (T item in source)
            {
                cnt++;
                if (cnt == count)
                {
                    cnt = 0;
                    yield return item;
                    action(item);
                }
            }
        }
    }
}
