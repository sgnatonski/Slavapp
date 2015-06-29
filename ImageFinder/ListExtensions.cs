using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFinder
{
    public static class ListExtenstions
    {
        public static IEnumerable<ushort[]> GetIndexCombinations(this ushort value)
        {
            for (int i = 0; i < value; i++)
            {
                for (int j = i + 1; j < value; j++)
                {
                    yield return new[] { (ushort)i, (ushort)j };
                }
            }
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

        public static IEnumerable<List<T>> Partition<T>(this IEnumerable<T> source, Int32 size)
        {
            for (int i = 0; i < Math.Ceiling(source.Count() / (Double)size); i++)
                yield return new List<T>(source.Skip(size * i).Take(size));
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }
    }
}
