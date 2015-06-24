using LinqSpecifications.Filtering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqFiltering.Specification
{
    public abstract class Specification<TEntity> where TEntity : class
    {
        protected IQueryable<TEntity> query;

        public IQueryable<TEntity> AsQueryable()
        {
            return this.query;
        }

        public List<TEntity> ToList()
        {
            return this.query.ToList();
        }

        public Specification<TEntity> WithFilter(IBaseFilter<TEntity> filter)
        {
            this.query = filter.Filter(this.query);
            return this;
        }

        public Specification<TEntity> WithSorting(IBaseSorting<TEntity> sorting)
        {
            this.query = sorting.Sort(this.query);
            return this;
        }
    }
}
