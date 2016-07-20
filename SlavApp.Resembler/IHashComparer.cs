using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Resembler
{
    public delegate void CompareProgressEventHandler(string file1, Distance[] file2);
    public interface IHashComparer
    {
        event CompareProgressEventHandler OnCompareProgress;

        void Run(IEnumerable<string> files, int distance);

        void Run(IEnumerable<string> files, int distance, Func<bool> continueTest);
    }
}
