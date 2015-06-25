using ImageFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var sf = new SimilarFinder();
            sf.Run(@"R:\APART_ALL\ZDJĘCIA EXPO", "*.jpg", 0.6).Wait();
        }
    }
}
