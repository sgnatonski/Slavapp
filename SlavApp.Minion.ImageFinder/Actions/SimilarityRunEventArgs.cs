using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.ImageFinder.Actions
{
    public class SimilarityRunEventArgs : EventArgs
    {
        public SimilarityRunEventArgs(long total, string file1, string[] file2, double value)
        {
            Total = total;
            File1 = file1;
            File2 = file2;
            Value = value;
        }
        public long Total { get; private set; }
        public string File1 { get; private set; }
        public string[] File2 { get; private set; }
        public double Value { get; private set; }
    }
}
