using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqSpecifications.Filtering
{
    public abstract class BaseSorting<TModel> : IBaseSorting<TModel>
    {
        protected readonly List<SortCommand> sorts = new List<SortCommand>();

        public BaseSorting(string sortString)
        {
            var cols = sortString.Split('|');
            foreach (var col in cols)
            {
                var sort = col.Split(':');
                sorts.Add(new SortCommand() { Column = sort[0], Ascending = sort[1] == "asc" });
            }
        }

        public abstract IQueryable<TModel> Sort(IQueryable<TModel> set);
    }
}
