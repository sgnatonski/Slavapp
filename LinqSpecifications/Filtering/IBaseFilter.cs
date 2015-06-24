using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqSpecifications.Filtering
{
    public interface IBaseFilter<TModel>
    {
        IQueryable<TModel> Filter(IQueryable<TModel> set);
    }
}
