using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqFiltering
{
    public interface IAddable<T> : IQueryable<T>
    {
        T Add(T entity);
    }
}
