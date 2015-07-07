using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImageFinder
{
    public static class Utils
    {
        /*public static BigInteger Factorial(BigInteger num)
        {
            BigInteger result = num;

            for (BigInteger i = 1; i < num; i++)
            {
                result = result * i;
            }
            return result;
        }*/

        public static string GetAssemblyPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            return path;
        }

        /*public static long GetPermutationsCount(long itemsCount, int permutations)
        {
            var n = Utils.Factorial(itemsCount);
            var r = Utils.Factorial(itemsCount - permutations);
            var total = (long)(n / r);
            return total;
        }*/
    }
}
