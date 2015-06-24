using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqFiltering.Resources
{
    public class Resources
    {
        private JObject r;

        public Resources(string path)
        {
            using (var reader = new StreamReader(path))
            {
                string json = reader.ReadToEnd();
                this.r = JObject.Parse(json);
            }
        }

        public TReturn GetResourceValue<TReturn>(Enum type)
        {
            return r[type.GetType().Name][((int)(object)type).ToString()].Value<TReturn>();
        }

        public string GetResourceValue(string resource, string type, string prop, string value)
        {
            var item = r[resource].AsJEnumerable().FirstOrDefault(x => x[type].Value<string>() == value);
            if (item == null)
                return null;
            return item[prop].Value<string>();
        }
    }
}
