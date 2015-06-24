using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqFiltering.Specification
{
    public sealed class SpecificableDbSet<T, TSpec> : IDbSet<T>, IAddable<T>
        where T : class
        where TSpec : Specification<T>
    {
        private readonly DbSet<T> dbSet;

        public SpecificableDbSet(DbContext context)
            : this(context.Set<T>())
        {
        }

        private SpecificableDbSet(DbSet<T> set)
        {
            this.dbSet = set;
        }

        private TSpec ConstructSpec(IQueryable<T> set)
        {
            var constructor = typeof(TSpec).GetConstructor(new[] { typeof(IQueryable<T>) });
            if (constructor == null)
            {
                throw new Exception();
            }

            return (TSpec)constructor.Invoke(new[] { set });
        }

        public TSpec Specify
        {
            get
            {
                return this.ConstructSpec(this.dbSet);
            }
        }

        public TSpec SpecifyNoTrack
        {
            get
            {
                return this.ConstructSpec(this.dbSet.AsNoTracking());
            }
        }

        #region IQueryable<T> members

        public IEnumerator<T> GetEnumerator()
        {
            return ((IQueryable<T>)this.dbSet).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IQueryable<T>)this.dbSet).GetEnumerator();
        }

        public Type ElementType
        {
            get { return ((IQueryable<T>)this.dbSet).ElementType; }
        }

        public Expression Expression
        {
            get { return ((IQueryable<T>)this.dbSet).Expression; }
        }

        public IQueryProvider Provider
        {
            get { return ((IQueryable<T>)this.dbSet).Provider; }
        }

        #endregion IQueryable<T> members

        #region IDbSet<T> members

        public T Add(T entity)
        {
            return this.dbSet.Add(entity);
        }

        public T Attach(T entity)
        {
            return this.dbSet.Attach(entity);
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            return this.dbSet.Create<TDerivedEntity>();
        }

        public T Create()
        {
            return this.dbSet.Create<T>();
        }

        public T Find(params object[] keyValues)
        {
            return this.dbSet.Find(keyValues);
        }

        public System.Collections.ObjectModel.ObservableCollection<T> Local
        {
            get { return this.dbSet.Local; }
        }

        public T Remove(T entity)
        {
            return this.dbSet.Remove(entity);
        }

        #endregion IDbSet<T> members

        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            return this.dbSet.AddRange(entities);
        }

        public IEnumerable<T> RemoveRange(IEnumerable<T> entities)
        {
            return this.dbSet.RemoveRange(entities);
        }

        public IQueryable<T> Include<TProperty>(Expression<Func<T, TProperty>> path)
        {
            return this.dbSet.Include(path);
        }
    }
}
