using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Dynamic;
using System.Dynamic;
using Microsoft.EntityFrameworkCore.Internal;

namespace EFPosh
{
    public class PoshEntityQueryBase<T> : DynamicObject
        where T : class
    {
        internal IQueryable<T> _baseIQueryable;
        internal string _query;
        internal DbContext _baseContext;
        internal List<object> _whereParams;
        public PoshEntityQueryBase(DbContext context, string WhereQuery = "", List<object> whereParams = null)
        {
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
        public PoshEntityQueryBase<T> Distinct()
        {
            _baseIQueryable = _baseIQueryable.Distinct();
            return this;
        }
        public List<T> ToList()
        {
            if(!string.IsNullOrEmpty(_query))
            {
                _baseIQueryable = _baseIQueryable.Where(_query, _whereParams.ToArray());
            }
            return _baseIQueryable.ToList();
        }
        public T FirstOrDefault()
        {
            if (!string.IsNullOrEmpty(_query))
            {
                _baseIQueryable.Where(_query, _whereParams.ToArray());
            }
            return _baseIQueryable.FirstOrDefault();
        }
        public bool Any()
        {
            if (!string.IsNullOrEmpty(_query))
            {
                _baseIQueryable.Where(_query, _whereParams.ToArray());
            }
            return _baseIQueryable.Any();
        }
    }
    public class PoshEntityColumn<T> : PoshEntityQueryBase<T>
        where T : class
    {
        public Dictionary<string, object> _members;
        public PoshEntityColumn(DbContext dbContext, string WhereQuery, List<object> whereParams):base(dbContext, WhereQuery, whereParams)
        {
            _members = new Dictionary<string, object>();
            var props = typeof(T).GetProperties();
            foreach(var p in props)
            {
                _members.Add(p.Name.ToLower(), new PoshEntityQuery<T>(p.Name, _query, _baseContext, _whereParams));
            }
        }
        
        public override bool TryGetMember(GetMemberBinder binder,
                                  out object result)
        {
            
            string name = binder.Name.ToLower();
            return _members.TryGetValue(name, out result);
        }
    }
    public class PoshEntityQuery<T> where T : class
    {
        private string _whereQuery;
        private string _columnName;
        private DbContext _dbContext;
        private List<object> _whereParams;
        public PoshEntityQuery(string columnName, string WhereQuery, DbContext dbContext, List<object> whereParams)
        {
            _whereQuery = WhereQuery;
            _columnName = columnName;
            _dbContext = dbContext;
            _whereParams = whereParams;
        }
        private PoshEntityJoiner<T> GetReturnObject()
        {
            return new PoshEntityJoiner<T>(_dbContext, _whereQuery, _whereParams);
        }
        public new PoshEntityJoiner<T> Equals(object equalValue)
        {
            _whereQuery += $"{_columnName}=@{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityJoiner<T> NotEquals(object equalValue)
        {
            _whereQuery += $"{_columnName}!=@{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityJoiner<T> Contains(object equalValue)
        {
            _whereQuery += $"{_columnName}.Contains(@{_whereParams.Count}) ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityJoiner<T> NotContains(object equalValue)
        {
            _whereQuery += $"!{_columnName}.Contains(@{_whereParams.Count}) ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityJoiner<T> StartsWith(object equalValue)
        {
            _whereQuery += $"{_columnName}.StartsWith(@{_whereParams.Count}) ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityJoiner<T> EndsWith(object equalValue)
        {
            _whereQuery += $"{_columnName}.EndsWith(@{_whereParams.Count}) ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityJoiner<T> GreaterThan(object equalValue)
        {
            _whereQuery += $"{_columnName} > @{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityJoiner<T> LessThan(object equalValue)
        {
            _whereQuery += $"{_columnName} < @{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityJoiner<T> GreaterThanOrEqualTo(object equalValue)
        {
            _whereQuery += $"{_columnName} >= @{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
        public PoshEntityJoiner<T> LessThanOrEqualTo(object equalValue)
        {
            _whereQuery += $"{_columnName} <= @{_whereParams.Count} ";
            _whereParams.Add(equalValue);
            return GetReturnObject();
        }
    }
    public class PoshEntityJoiner<T> : PoshEntityQueryBase<T>
        where T : class
    {
        public PoshEntityJoiner(DbContext dbContext, string WhereQuery, List<object> whereParams) : base(dbContext, WhereQuery, whereParams) { }
        private PoshEntityColumn<T> GetReturnValue()
        {
            return new PoshEntityColumn<T>(_baseContext, _query, _whereParams);
        }
        public PoshEntityColumn<T> And
        {
            get
            {
                _query += " && ";
                return GetReturnValue();
            }
        }
        
        public PoshEntityColumn<T> Or
        {
            get
            {
                _query += " || ";
                return GetReturnValue();
            }
        }
    }
}
