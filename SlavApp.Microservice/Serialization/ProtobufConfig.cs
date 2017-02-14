using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf.Meta;
using SlavApp.Microservice.Requests;
using System.Linq.Expressions;
using System.Reflection;

namespace SlavApp.Microservice.Serialization
{
    internal static class ProtobufConfig
    {
        static object syncRoot = new object();
        static bool initialized = false;
        public static void Initialize()
        {
            lock (syncRoot)
            {
                if (!initialized)
                {
                    MapProperties<ServerTopicHeader>(
                        c => c.PropertyName(x => x.WorkerId),
                        c => c.PropertyName(x => x.Topic));
                    MapProperties<ContextMessageSurrogate>(
                        c => c.PropertyName(x => x.User),
                        c => c.PropertyName(x => x.CreatedAt),
                        c => c.PropertyName(x => x.RawData));
                    MapProperties<ContextMessage>()
                        .SetSurrogate(typeof(ContextMessageSurrogate));
                    MapProperties<ResponseContextMessage>(
                        c => c.PropertyName(x => x.Message),
                        c => c.PropertyName(x => x.ResponsePort))
                        .UseConstructor = false;
                }
            }
        }

        private static MetaType MapProperties<T>(params Func<PropertyMapper<T>, string>[] properties)
        {
            var mapper = new PropertyMapper<T>();
            var metaType = RuntimeTypeModel.Default.Add(typeof (T), false);

            foreach (var property in properties)
            {
                metaType = metaType.Add(property(mapper));
            }

            return metaType;
        }
    }
}
