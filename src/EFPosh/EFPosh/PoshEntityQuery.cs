using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if NETFRAMEWORK
using System.Linq.Dynamic;
#else
using System.Linq.Dynamic.Core;
#endif
using System.Dynamic;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EFPosh
{
    public class PoshEntityQueryBase<T> : DynamicObject
        where T : class
    {
        internal IQueryable<T> _baseIQueryable;
        internal string _query;
        internal DbContext _baseContext;
        internal List<object> _whereParams;
        internal string _fromSql;
        internal List<object> _fromSqlParams;
        internal ActionRunner _runner;
        internal string[] _selectProperties;
        public PoshEntityQueryBase(DbContext context, ActionRunner runner, string WhereQuery = "", List<object> whereParams = null, string fromSql = "", List<object>fromSqlParams = null)
        {
            _runner = runner;
            var ets = context.Model.GetEntityTypes();
            foreach(var et in ets)
            {
                if(et.ClrType == typeof(T))
                {
                    if (et.IsQueryType)
                    {
                        _baseIQueryable = context.Query<T>().AsQueryable();
                    }
                    else
                    {
                        _baseIQueryable = context.Set<T>().AsQueryable();
                    }
                }
            }

            _baseContext = context;
            _query = WhereQuery;
            if(whereParams == null)
            {
                _whereParams = new List<object>();
            }
            else
            {
                _whereParams = whereParams;
            }
            
            _fromSql = fromSql;
            _fromSqlParams = fromSqlParams;
        }
        public List<string> GetProperties()
        {
            return typeof(T).GetProperties().Select(p => p.Name).ToList();
        }
        public PoshEntityQueryBase<T> AsNoTracking()
        {
            _baseIQueryable = _baseIQueryable.AsNoTracking();
            return this;
        }
        public PoshEntityQueryBase<T> Take(int take)
        {
            _baseIQueryable = _baseIQueryable.Take(take);
            return this;
        }
        public PoshEntityQueryBase<T> Skip(int skip)
        {
            _baseIQueryable = _baseIQueryable.Skip(skip);
            return this;
        }
        public PoshEntityQueryBase<T> OrderBy(string orderBy)
        {
            _baseIQueryable = _baseIQueryable.OrderBy(orderBy);
            return this;
        }

        public PoshEntityQueryBase<T> Include(string includeName)
        {
            _baseIQueryable = _baseIQueryable.Include(includeName);
            return this;
        }
        public PoshEntityQueryBase<T> Distinct()
        {
            _baseIQueryable = _baseIQueryable.Distinct();
            return this;
        }
        private Expression<Func<T,T>> CreateSelectLambda(string[] properties)
        {
            var xParameter = Expression.Parameter(typeof(T), "o");
            var xNew = Expression.New(typeof(T));
            List<MemberAssignment> assignments = new List<MemberAssignment>();
            foreach (var prop in properties)
            {
                var mi = typeof(T).GetProperties()
                    .Where(p => p.Name.ToLower().Equals(prop.ToLower()))
                    .FirstOrDefault();
                var xOriginal = Expression.Property(xParameter, mi);
                assignments.Add(Expression.Bind(mi, xOriginal));
            }
            var xInit = Expression.MemberInit(xNew, assignments);
            return Expression.Lambda<Func<T, T>>(xInit, xParameter);
        }
        public PoshEntityQueryBase<T> Select(string[] properties)
        {
            _selectProperties = properties;
            return this;
        }
        public PoshEntityColumn<T> And
        {
            get
            {
                _query += " && ";
                return new PoshEntityColumn<T>(_baseContext, _runner, _query, _whereParams, _fromSql, _fromSqlParams);
            }
        }
        public PoshEntityColumn<T> Or
        {
            get
            {
                _query += " || ";
                return new PoshEntityColumn<T>(_baseContext, _runner, _query, _whereParams, _fromSql, _fromSqlParams);
            }
        }
        public PoshEntityQueryBase<T> FromSql(string query, object[] objParams = null)
        {
            _fromSql = query;
            foreach(var obj in objParams)
            {
                _fromSqlParams.Add(obj);
            }
            return this;
        }
        public PoshEntityQueryBase<T> FromSql(string query)
        {
            _fromSql = query;
            return this;
        }
        private void UpdateIQueryable()
        {
            if (!string.IsNullOrEmpty(_fromSql))
            {
                _baseIQueryable = _baseIQueryable.FromSql(_fromSql, _fromSqlParams.ToArray());
            }
            if (!string.IsNullOrEmpty(_query))
            {
                _baseIQueryable = _baseIQueryable.Where(_query, _whereParams.ToArray());
            }
            if (_selectProperties != null)
            {
                _baseIQueryable = _baseIQueryable.Select((CreateSelectLambda(_selectProperties)));
            }
        }
        public List<T> ToList()
        {
            UpdateIQueryable();
            return _runner.RunAction(() => _baseIQueryable.ToList());
        }
        public T FirstOrDefault()
        {
            UpdateIQueryable();
            return _runner.RunAction(() => _baseIQueryable.FirstOrDefault());
        }
        public bool Any()
        {
            UpdateIQueryable();
            return _runner.RunAction(() => _baseIQueryable.Any());
        }
        public IQueryable<T> GetQueryable()
        {
            return _baseIQueryable;
        }
        public void ApplyExpression(Expression<Func<T, bool>> exp)
        {
            _baseIQueryable = _baseIQueryable.Where(exp);
        }
    }
    public class PoshEntityColumn<T> : PoshEntityQueryBase<T>
        where T : class
    {
        public PoshEntityColumn(DbContext dbContext, ActionRunner runner, string WhereQuery, List<object> whereParams, string fromSql, List<object> fromSqlParams):base(dbContext, runner, WhereQuery, whereParams, fromSql, fromSqlParams)
        {

        }
        
        public override bool TryGetMember(GetMemberBinder binder,
                                  out object result)
        {
            result = null;
            string name = binder.Name.ToLower();
            var propObject = typeof(T).GetProperties().Where(p => p.Name.ToLower().Equals(name)).FirstOrDefault();
            if(propObject != null)
            {
                result = new PoshEntityQuery<T>(propObject.Name, _runner, _query, _baseContext, _whereParams, _fromSql, _fromSqlParams);
                return true;
            }
            return false;
        }

        public T New()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        public Type GetBaseType()
        {
            return typeof(T);
        }
    }
    public class PoshEntityQuery<T> where T : class
    {
        private string _whereQuery;
        private string _columnName;
        private DbContext _dbContext;
        private List<object> _whereParams;
        private string _fromSql;
        private List<object> _fromSqlParams;
        private ActionRunner _runner;
        public PoshEntityQuery(string columnName, ActionRunner runner, string WhereQuery, DbContext dbContext, List<object> whereParams, string fromSql, List<object> fromSqlParams)
        {
            _runner = runner;
            _whereQuery = WhereQuery;
            _columnName = columnName;
            _dbContext = dbContext;
            _whereParams = whereParams;
            _fromSql = fromSql;
            _fromSqlParams = fromSqlParams;
        }
        private PoshEntityQueryBase<T> GetReturnObject()
        {
            return new PoshEntityQueryBase<T>(_dbContext, _runner, _whereQuery, _whereParams, _fromSql, _fromSqlParams);
        }
        public new PoshEntityQueryBase<T> Equals(object equalValue)
        {
            _whereQuery += $"{_columnName}=@{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityQueryBase<T> NotEquals(object equalValue)
        {
            _whereQuery += $"{_columnName}!=@{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityQueryBase<T> Contains(object equalValue)
        {
            var equalValueType = equalValue.GetType();
            if (equalValueType.IsArray)
            {
                _whereQuery += $"@{_whereParams.Count}.Contains(outerIt.{_columnName}) ";
                _whereParams.Add(equalValue);
            }
            else
            {
                _whereQuery += $"{_columnName}.Contains(@{_whereParams.Count}) ";
                _whereParams.Add(equalValue);
            }
            return GetReturnObject();
        }
        public PoshEntityQueryBase<T> NotContains(object equalValue)
        {
            var equalValueType = equalValue.GetType();
            if (equalValueType.IsArray)
            {
                _whereQuery += $"!@{_whereParams.Count}.Contains(outerIt.{_columnName}) ";
                _whereParams.Add(equalValue);
            }
            else
            {
                _whereQuery += $"!{_columnName}.Contains(@{_whereParams.Count}) ";
                _whereParams.Add(equalValue);
            }
            return GetReturnObject();
        }
        public PoshEntityQueryBase<T> StartsWith(object equalValue)
        {
            _whereQuery += $"{_columnName}.StartsWith(@{_whereParams.Count}) ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityQueryBase<T> EndsWith(object equalValue)
        {
            _whereQuery += $"{_columnName}.EndsWith(@{_whereParams.Count}) ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityQueryBase<T> GreaterThan(object equalValue)
        {
            _whereQuery += $"{_columnName} > @{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityQueryBase<T> LessThan(object equalValue)
        {
            _whereQuery += $"{_columnName} < @{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityQueryBase<T> GreaterThanOrEqualTo(object equalValue)
        {
            _whereQuery += $"{_columnName} >= @{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityQueryBase<T> LessThanOrEqualTo(object equalValue)
        {
            _whereQuery += $"{_columnName} <= @{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
    }

}
