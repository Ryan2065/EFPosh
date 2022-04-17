using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
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

        public void Include(string propertyName, string thenInclude = "")
        {
            
            var propertyInfo = typeof(T).GetProperties().Where(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).First();

            if (string.IsNullOrEmpty(thenInclude))
            {
                var methodInfo = this.GetType().GetMethod("IncludeInternal", BindingFlags.NonPublic | BindingFlags.Instance);
                var gMethod = methodInfo.MakeGenericMethod(propertyInfo.PropertyType);
                gMethod.Invoke(this, new[] { propertyInfo.Name });
            }
            else
            {
                var methodInfo = this.GetType().GetMethod("ThenIncludeInternal", BindingFlags.NonPublic | BindingFlags.Instance);
                var propType = propertyInfo.PropertyType;
                if (propType.GetGenericArguments().Count() == 1)
                {
                    propType = propertyInfo.PropertyType.GetGenericArguments()[0];
                }
                var thenPropertyInfo = propType.GetProperties().Where(p => p.Name.Equals(thenInclude, StringComparison.OrdinalIgnoreCase)).First();
                var gMethod = methodInfo.MakeGenericMethod(propertyInfo.PropertyType, propType, thenPropertyInfo.PropertyType);
                gMethod.Invoke(this, new[] { propertyInfo.Name, thenPropertyInfo.Name });
            }
        }

        internal void IncludeInternal<TKey>(string propertyName)
        {
            _modifiedIQueryable = _modifiedIQueryable.Include(GetSinglePropertyExpression<T, TKey>(propertyName));
        }

        internal void ThenIncludeInternal<TKey, TKeyNotEnumerable, TSecondKey>(string propertyName, string thenIncludeProperty)
        {
            if (typeof(TKey).GetGenericArguments().Count() == 1)
            {
                _modifiedIQueryable = _modifiedIQueryable.Include(GetSinglePropertyExpression<T, IEnumerable<TKeyNotEnumerable>>(propertyName)).ThenInclude(GetSinglePropertyExpression<TKeyNotEnumerable, TSecondKey>(thenIncludeProperty));
            }
            else
            {
                _modifiedIQueryable = _modifiedIQueryable.Include(GetSinglePropertyExpression<T, TKey>(propertyName)).ThenInclude(GetSinglePropertyExpression<TKey, TSecondKey>(thenIncludeProperty));
            }
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

        internal Expression<Func<TKey, TKey2>> GetSinglePropertyExpression<TKey, TKey2>(string propertyName)
        {
            var newParam = Expression.Parameter(typeof(TKey), "newP");
            var body = Expression.Property(newParam, propertyName);
            ParameterExpression[] array = new ParameterExpression[1];
            array[0] = newParam;
            return Expression.Lambda<Func<TKey, TKey2>>(body, array);
        }

        internal void OrderByInternal<TKey>(string propertyName)
        {
            _modifiedIQueryable = _modifiedIQueryable.OrderBy(GetSinglePropertyExpression<T, TKey>(propertyName));
        }

        internal void OrderByDescendingInternal<TKey>(string propertyName)
        {
            _modifiedIQueryable = _modifiedIQueryable.OrderByDescending(GetSinglePropertyExpression<T, TKey>(propertyName));
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
