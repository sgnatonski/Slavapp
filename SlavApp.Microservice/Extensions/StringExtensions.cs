using ProtoBuf;
using ProtoBuf.Meta;
using SlavApp.Microservice.Requests;
using SlavApp.Microservice.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Microservice.Extensions
{
    public static class StringExtensions
    {
        public static T Deserialize<T>(this byte[] str)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(str, 0, str.Length);
                stream.Position = 0;
                return Serializer.Deserialize<T>(stream);
            }
        }

        public static byte[] Serialize<T>(this T obj)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, obj);
                return stream.ToArray();
            }
        }
    }
}
