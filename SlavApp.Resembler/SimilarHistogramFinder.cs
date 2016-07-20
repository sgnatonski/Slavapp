using DBreeze;
using DBreeze.Transactions;
using System;
using System.IO;
using System.Linq;

namespace SlavApp.Resembler
{
    public class SimilarHistogramFinder
    {
        private readonly DBreezeConfiguration dbConf = new DBreezeConfiguration()
        {
            DBreezeDataFolderName = Path.Combine(Utils.GetAssemblyPath(), "DBR"),
            Storage = DBreezeConfiguration.eStorage.DISK
        };

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
            var allfiles = System.IO.Directory.GetFiles(directory, filter, System.IO.SearchOption.AllDirectories);

            using (var engine = new DBreezeEngine(dbConf))
            {
                allfiles.AsParallel().ForAll(f =>
                {
                    if (continueTest())
                    {
                        using (var tran = engine.GetTransaction())
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
                    list.AsParallel().ForAll(c =>
                    {
                        if (!continueTest())
                        {
                            return;
                        }
                        var f = allfiles[c[0]];
                        var l = allfiles[c[1]];
                        var value = 0.0;
                        using (var tran = engine.GetTransaction())
                        {
                            tran.ValuesLazyLoadingIsOn = false;
                            value = GetComparisonResult(tran, f, l);
                            tran.Commit();
                        }
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
                    });
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
                tran.Insert("hist", first, new HistogramData(pc.Projections.HorizontalProjection, pc.Projections.VerticalProjection));
            }
            else
            {
                pc = new ComparableImage(new FileInfo(first), pcData.Value.X, pcData.Value.Y);
            }
            return pc;
        }
    }
}