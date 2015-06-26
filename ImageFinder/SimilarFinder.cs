using BookSleeve;
using EyeOpen.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFinder
{
    public class SimilarFinder
    {
        public delegate void ProgressEventHandler(int total, string file1, string file2, double value);

        public event ProgressEventHandler OnProgress;

        public async Task<List<SimilarityResult>> Run(string directory, string filter, double minSimilarity)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            var allfiles = System.IO.Directory.GetFiles(directory, filter, System.IO.SearchOption.AllDirectories);

            var list = allfiles.GetPermutations(2);

            using (var conn = new RedisConnection("localhost"))
            {
                await conn.Open();

                var total = list.Count();
                var results = list.AsParallel().Select(comp => GetComparisonResult(conn, comp, total, minSimilarity).Result).Where(x => x.Value >= minSimilarity).ToList();

                return results;
            }
        }

        private async Task<SimilarityResult> GetComparisonResult(RedisConnection conn, IEnumerable<string> comp, int total, double minValue)
        {
            var f = comp.First();
            var s = comp.Last();
            ComparableImage pc = null;
            var pcData = await conn.Strings.Get(1, f);
            if (pcData == null)
            {
                pc = new ComparableImage(new FileInfo(f));
                await conn.Strings.Set(1, f, pc.ToByteArray());
                Debug.Write("+");
            }
            else
            {
                pc = new ComparableImage(new FileInfo(f), pcData);
            }

            ComparableImage cc = null;
            var ccData = await conn.Strings.Get(1, s);
            if (ccData == null)
            {
                cc = new ComparableImage(new FileInfo(s));
                await conn.Strings.Set(1, s, cc.ToByteArray());
                Debug.Write("+");
            }
            else
            {
                cc = new ComparableImage(new FileInfo(s), ccData);
            }

            /*if (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 > 100 * 1024 * 1024)
            {
                GC.Collect();
                Debug.WriteLine("Gc.Collect {0}", System.Diagnostics.Process.GetCurrentProcess().WorkingSet64);
            }*/

            var value = pc.CalculateSimilarity(cc);
            if (value >= minValue && OnProgress != null)
            {
                OnProgress(total, f, s, value);
            }

            return new SimilarityResult { First = f, Second = s, Value = value };
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
