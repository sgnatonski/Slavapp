using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Resembler
{
    public delegate void ProgressEventHandler(string file1);

    public interface IHashCalculator
    {
        event ProgressEventHandler OnProgress;

        void Run(IEnumerable<string> files);

        void Run(IEnumerable<string> files, Func<bool> continueTest, Func<bool> pauseTest);
    }
}
