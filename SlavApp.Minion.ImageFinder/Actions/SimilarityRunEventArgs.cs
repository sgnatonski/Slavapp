using SlavApp.ImageFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.ImageFinder.Actions
{
    public class SimilarityRunEventArgs : EventArgs
    {
        public SimilarityRunEventArgs(long total, Distance[] files)
        {
            Total = total;
            Files = files;
        }
        public long Total { get; private set; }
        public Distance[] Files { get; private set; }
    }
}
