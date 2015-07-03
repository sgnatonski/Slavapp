﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFinder
{
    public static class ListExtenstions
    {
        public static T DeserializeProtobuf<T>(this byte[] data)
        {
            T ret = default(T);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(data))
            {
                ret = ProtoBuf.Serializer.Deserialize<T>(ms);
                ms.Close();
            }
            return ret;
        }
        public static object DeserializeProtobuf(byte[] data, Type T)
        {
            object ret = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(data))
            {
                ret = ProtoBuf.Serializer.NonGeneric.Deserialize(T, ms);
                ms.Close();
            }
            return ret;
        }
        public static byte[] SerializeProtobuf(this object data)
        {
            byte[] bt = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ProtoBuf.Serializer.NonGeneric.Serialize(ms, data);
                bt = ms.ToArray();
                ms.Close();
            }
            return bt;
        }

        public static Int64 GetInt64HashCode(this string strText)
        {
            Int64 hashCode = 0;
            if (!string.IsNullOrEmpty(strText))
            {
                //Unicode Encode Covering all characterset
                byte[] byteContents = Encoding.Unicode.GetBytes(strText);
                System.Security.Cryptography.SHA256 hash = new System.Security.Cryptography.SHA256CryptoServiceProvider();
                byte[] hashText = hash.ComputeHash(byteContents);
                //32Byte hashText separate
                //hashCodeStart = 0~7  8Byte
                //hashCodeMedium = 8~23  8Byte
                //hashCodeEnd = 24~31  8Byte
                //and Fold
                Int64 hashCodeStart = BitConverter.ToInt64(hashText, 0);
                Int64 hashCodeMedium = BitConverter.ToInt64(hashText, 8);
                Int64 hashCodeEnd = BitConverter.ToInt64(hashText, 24);
                hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            }
            return (hashCode);
        }

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