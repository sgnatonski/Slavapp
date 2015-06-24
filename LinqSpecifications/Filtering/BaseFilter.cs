using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqSpecifications.Filtering
{
    public abstract class BaseFilter<TModel> : IBaseFilter<TModel>
    {
        protected readonly List<FilterCommand> filters = new List<FilterCommand>();

        public BaseFilter(string filterString)
        {
            if (string.IsNullOrEmpty(filterString))
                return;
            var cols = filterString.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var col in cols)
            {
                var sort = col.Split(':');
                filters.Add(new FilterCommand()
                {
                    Independent = sort[0].StartsWith("!"),
                    Column = sort[0].Replace("!", ""),
                    Filter = sort[1].Substring(1),
                    Operation = GetOperation(sort[1][0])
                });

                filters = filters.OrderByDescending(x => x.Independent).ToList();
            }
        }

        private FilterOperation GetOperation(char o)
        {
            switch (o)
            {
                case '^': return FilterOperation.StartsWith;
                case '*': return FilterOperation.Equal;
                case '>': return FilterOperation.GreaterThan;
                case '<': return FilterOperation.LesserThan;
                case '!': return FilterOperation.NotEqual;
                case '~': return FilterOperation.Contains;
                default: return FilterOperation.StartsWith;
            }
        }

        protected Expression<Func<T, bool>> GetPredicate<T, TType>(Expression<Func<T, TType>> expr, FilterCommand command)
        {
            var argParam = expr.Parameters[0];
            var val1 = Expression.Constant(Convert.ChangeType(command.Filter, typeof(TType)));

            Expression e1 = null;
            switch (command.Operation)
            {
                case FilterOperation.Equal:
                    e1 = Expression.Equal(expr.Body, val1); break;
                case FilterOperation.NotEqual:
                    e1 = Expression.NotEqual(expr.Body, val1); break;
                case FilterOperation.GreaterThan:
                    e1 = Expression.GreaterThan(expr.Body, val1); break;
                case FilterOperation.LesserThan:
                    e1 = Expression.LessThan(expr.Body, val1); break;
                case FilterOperation.StartsWith:
                    e1 = Expression.Call(expr.Body, typeof(TType).GetMethod("ToString", new Type[] { }));
                    e1 = Expression.Call(e1, typeof(string).GetMethod("ToLower", new Type[] { }));
                    e1 = Expression.Call(e1, typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }), Expression.Constant(command.Filter.ToLower()));
                    break;

                case FilterOperation.Contains:
                    e1 = Expression.Call(expr.Body, typeof(TType).GetMethod("ToString", new Type[] { }));
                    e1 = Expression.Call(e1, typeof(string).GetMethod("ToLower", new Type[] { }));
                    e1 = Expression.Call(e1, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), Expression.Constant(command.Filter.ToLower()));
                    break;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(e1, argParam);
            return lambda;
        }

        protected IQueryable<T> ApplyOperation<T, TType>(IQueryable<T> set, Expression<Func<T, TType>> expr, FilterCommand command)
        {
            return set.Where(GetPredicate(expr, command));
        }

        public abstract IQueryable<TModel> Filter(IQueryable<TModel> set);
    }
}
