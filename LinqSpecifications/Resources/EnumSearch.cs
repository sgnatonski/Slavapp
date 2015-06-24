using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqFiltering.Resources
{
    public class EnumSearch
    {
        private readonly Resources resources;
        public EnumSearch(Resources resources)
        {
            this.resources = resources;
        }

        public List<EnumType> GetMatchingValues<EnumType>(string match)
        {
            var translations = new Dictionary<string, EnumType>();

            var vals = typeof(EnumType).GetEnumValues();
            foreach (EnumType v in vals)
            {
                translations.Add(this.resources.GetResourceValue<string>((Enum)(object)v), v);
            }

            var keys = translations.Where(x => x.Key.ToLower().Contains(match.ToLower()));

            return keys.Select(x => x.Value).ToList();
        }
    }
}
