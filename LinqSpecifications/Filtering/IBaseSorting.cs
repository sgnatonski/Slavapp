using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqSpecifications.Filtering
{
    public interface IBaseSorting<TModel>
    {
        IQueryable<TModel> Sort(IQueryable<TModel> set);
    }
}
