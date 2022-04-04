using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Expressions;
using System.Reflection;

namespace EFPosh
{
    public class PoshEntityColumn<T>
        where T : class
    {
        private IQueryable<T> _baseIQueryable;
        private IQueryable<T> _modifiedIQueryable;
        private ParameterExpression _p;
        private List<string> SelectProperties;
        private readonly DbContext _dbContext;
        public PoshEntityColumn(DbContext dbContext)
        {
            var ets = dbContext.Model.GetEntityTypes();
            foreach (var et in ets)
            {
                if (et.ClrType == typeof(T))
                {
#if NETFRAMEWORK
                    if (et.IsQueryType)
                    {
                        _baseIQueryable = dbContext.Query<T>().AsQueryable();
                    }
                    else
                    {
                        _baseIQueryable = dbContext.Set<T>().AsQueryable();
                    }
#else
                    _baseIQueryable = dbContext.Set<T>().AsQueryable();
#endif
                }
            }
            _modifiedIQueryable = _baseIQueryable;
            _p = Expression.Parameter(typeof(T), "p");
            SelectProperties = new List<string>();
            _dbContext = dbContext;
        }

        public T New()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        private Expression<Func<T, T>> CreateSelectLambda()
        {
            var xParameter = Expression.Parameter(typeof(T), "o");
            var xNew = Expression.New(typeof(T));
            List<MemberAssignment> assignments = new List<MemberAssignment>();
            foreach (var prop in SelectProperties)
            {
                var mi = typeof(T).GetProperties()
                    .Where(p => p.Name.ToLower().Equals(prop.ToLower()))
                    .First();
                var xOriginal = Expression.Property(xParameter, mi);
                assignments.Add(Expression.Bind(mi, xOriginal));
            }
            var xInit = Expression.MemberInit(xNew, assignments);
            SelectProperties.Clear();
            return Expression.Lambda<Func<T, T>>(xInit, xParameter);
        }

        private IQueryable<T> GetQueryableForExecution()
        {
            if(_modifiedIQueryable == null)
            {
                _modifiedIQueryable = _baseIQueryable.AsQueryable();
            }
            var tempQ = _modifiedIQueryable.AsQueryable();
            if(SelectProperties.Count > 0)
            {
                tempQ = tempQ.Select((CreateSelectLambda()));
            }
            _modifiedIQueryable = _baseIQueryable.AsQueryable();
            return tempQ;
        }

        public List<T> ToList()
        {
            return GetQueryableForExecution().ToList();
        }

        public bool Any()
        {
            return GetQueryableForExecution().Any();
        }

        public T FirstOrDefault()
        {
            return GetQueryableForExecution().FirstOrDefault();
        }

        public void AsNoTracking()
        {
            _modifiedIQueryable = _modifiedIQueryable.AsNoTracking();
        }

        public void FromSql(string query)
        {
#if NETFRAMEWORK
            _modifiedIQueryable = _modifiedIQueryable.FromSql(query);
#else
            // have to overwrite the query
            _modifiedIQueryable =  _dbContext.Set<T>().FromSqlRaw(query);
#endif
        }

        public void Include(string propertyName)
        {
            var methodInfo = this.GetType().GetMethod("IncludeInternal", BindingFlags.NonPublic | BindingFlags.Instance);
            var propertyInfo = typeof(T).GetProperties().Where(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var gMethod = methodInfo.MakeGenericMethod(propertyInfo.PropertyType);
            gMethod.Invoke(this, new[] { propertyInfo.Name });
        }

        internal void IncludeInternal<TKey>(string propertyName)
        {
            _modifiedIQueryable = _modifiedIQueryable.Include(GetSinglePropertyExpression<TKey>(propertyName));
        }

        public void Take(int take)
        {
             _modifiedIQueryable = _modifiedIQueryable.Take(take);
        }

        public void Skip(int skip)
        {
            _modifiedIQueryable = _modifiedIQueryable.Skip(skip);
        }

        public void Distinct()
        {
            _modifiedIQueryable = _modifiedIQueryable.Distinct();
        }

        public void Select(string[] names)
        {
            foreach(string name in names)
            {
                SelectProperties.Add(name);
            }
        }

        internal Expression<Func<T, TKey>> GetSinglePropertyExpression<TKey>(string propertyName)
        {
            
            var body = Expression.Property(_p, propertyName);
            ParameterExpression[] array = new ParameterExpression[1];
            array[0] = _p;
            return Expression.Lambda<Func<T, TKey>>(body, array);
        }

        internal void OrderByInternal<TKey>(string propertyName)
        {
            _modifiedIQueryable = _modifiedIQueryable.OrderBy(GetSinglePropertyExpression<TKey>(propertyName));
        }

        internal void OrderByDescendingInternal<TKey>(string propertyName)
        {
            _modifiedIQueryable = _modifiedIQueryable.OrderByDescending(GetSinglePropertyExpression<TKey>(propertyName));
        }

        public void OrderBy(string propertyName)
        {
            var methodInfo = this.GetType().GetMethod("OrderByInternal", BindingFlags.NonPublic | BindingFlags.Instance);
            var propertyInfo = typeof(T).GetProperties().Where(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var gMethod = methodInfo.MakeGenericMethod(propertyInfo.PropertyType);
            gMethod.Invoke(this, new[] { propertyInfo.Name });
        }

        public void OrderByDescending(string propertyName)
        {
            var methodInfo = this.GetType().GetMethod("OrderByDescendingInternal", BindingFlags.NonPublic | BindingFlags.Instance);
            var propertyInfo = typeof(T).GetProperties().Where(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var gMethod = methodInfo.MakeGenericMethod(propertyInfo.PropertyType);
            gMethod.Invoke(this, new[] { propertyInfo.Name });
        }

        public Type GetBaseType()
        {
            return typeof(T);
        }

        public IQueryable<T> GetQueryable()
        {
            return _baseIQueryable;
        }

        public void ApplyExpression(Expression<Func<T, bool>> exp)
        {
            _modifiedIQueryable = _modifiedIQueryable.Where(exp);
        }

        public void Reset()
        {
            _modifiedIQueryable = _baseIQueryable.AsQueryable();
            SelectProperties.Clear();
        }

    }

}
