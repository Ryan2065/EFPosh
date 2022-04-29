using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Management.Automation;
using PoshLogger;

namespace EFPosh
{
    /// <summary>
    /// Allows powershell to interact with DbSet and DbQuery types
    /// </summary>
    /// <typeparam name="T">Type of the DbSet or DbQuery we are interacting with</typeparam>
    public class PoshEntityInteractions<T>
        where T : class
    {
        private readonly IQueryable<T> _baseIQueryable;
        private IQueryable<T> _modifiedIQueryable;
        private readonly List<string> SelectProperties;
        private readonly PoshILogger _logger;
        private string _fromSql = "";
#if !NETFRAMEWORK
        private readonly DbContext _dbContext;
#endif
        /// <summary>
        /// Default constructor to create this class
        /// </summary>
        /// <param name="dbContext">DbContext the underlying entity is a part of</param>
        public PoshEntityInteractions(DbContext dbContext)
        {
            _logger = new PoshILogger(LogLevel.Trace);
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
                    _dbContext = dbContext;
#endif
                }
            }
            _modifiedIQueryable = _baseIQueryable;
            SelectProperties = new List<string>();
            
        }
        /// <summary>
        /// Creates a new object of the underlying entity type
        /// </summary>
        /// <returns>Returns a new object of the underlying entity type</returns>
        public T New()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }
        /// <summary>
        /// Creates an expression that will work in a Linq select query based off a list of Select items
        /// </summary>
        /// <returns>Correct expression</returns>
        private Expression<Func<T, T>> CreateSelectLambda()
        {
            var xParameter = Expression.Parameter(typeof(T), "o");
            var xNew = Expression.New(typeof(T));
            List<MemberAssignment> assignments = new();
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
        /// <summary>
        /// Gets a queryable of the base type with any cached "things"
        /// Cached items are things that need to be processed at the end, like
        /// the list of properties we want
        /// </summary>
        /// <returns>IQueryable ready to be run with .ToList or any other methods</returns>
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
            Reset();
            return tempQ;
        }
        /// <summary>
        /// Executes ToList on the built query
        /// </summary>
        /// <returns>List of objects</returns>
        public List<T> ToList()
        {
            return GetQueryableForExecution().ToList();
        }
        /// <summary>
        /// Executes Any on the built query
        /// </summary>
        /// <returns>Results of Any</returns>
        public bool Any()
        {
            return GetQueryableForExecution().Any();
        }
        /// <summary>
        /// Executes first or default on the built query
        /// </summary>
        /// <returns>Results of FirstOrDefault</returns>
        public T FirstOrDefault()
        {
            return GetQueryableForExecution().FirstOrDefault();
        }
        /// <summary>
        /// Marks the query as no tracking so change tracking won't be used
        /// </summary>
        public void AsNoTracking()
        {
            _modifiedIQueryable = _modifiedIQueryable.AsNoTracking();
        }
        /// <summary>
        /// Will set up the Entity to use a base query instead of entity framework coming up with the base
        /// </summary>
        /// <param name="query">Sql query to execute</param>
        public void FromSql(string query, bool IsAlwaysFromSQL = false)
        {
            if (IsAlwaysFromSQL)
            {
                _fromSql = query;
            }
#if NETFRAMEWORK
            _modifiedIQueryable = _modifiedIQueryable.FromSql(query);
#else
            // have to overwrite the query
            _modifiedIQueryable =  _dbContext.Set<T>().FromSqlRaw(query);
#endif
        }
        /// <summary>
        /// Sets up include properties to include reference properties
        /// </summary>
        /// <param name="propertyName">List of properties to include</param>
        /// <param name="thenInclude">List of reference properties to include</param>
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
                if (propType.GetGenericArguments().Length == 1)
                {
                    propType = propertyInfo.PropertyType.GetGenericArguments()[0];
                }
                var thenPropertyInfo = propType.GetProperties().Where(p => p.Name.Equals(thenInclude, StringComparison.OrdinalIgnoreCase)).First();
                var gMethod = methodInfo.MakeGenericMethod(propertyInfo.PropertyType, propType, thenPropertyInfo.PropertyType);
                gMethod.Invoke(this, new[] { propertyInfo.Name, thenPropertyInfo.Name });
            }
        }
        /// <summary>
        /// Internal method called with reflection to apply include
        /// </summary>
        /// <typeparam name="TKey">Type of the reference property being included</typeparam>
        /// <param name="propertyName">Name of the reference property</param>
        internal void IncludeInternal<TKey>(string propertyName)
        {
            _modifiedIQueryable = _modifiedIQueryable.Include(GetSinglePropertyExpression<T, TKey>(propertyName));
        }
        /// <summary>
        /// Internal method called with reflection to apply includes and theninclude
        /// </summary>
        /// <typeparam name="TKey">Type of the reference property</typeparam>
        /// <typeparam name="TKeyNotEnumerable">If reference property is a collection, it's base type</typeparam>
        /// <typeparam name="TSecondKey">Type of the then include</typeparam>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="thenIncludeProperty">Name of the then include</param>
        internal void ThenIncludeInternal<TKey, TKeyNotEnumerable, TSecondKey>(string propertyName, string thenIncludeProperty)
        {
            if (typeof(TKey).GetGenericArguments().Length == 1)
            {
                _modifiedIQueryable = _modifiedIQueryable.Include(GetSinglePropertyExpression<T, IEnumerable<TKeyNotEnumerable>>(propertyName)).ThenInclude(GetSinglePropertyExpression<TKeyNotEnumerable, TSecondKey>(thenIncludeProperty));
            }
            else
            {
                _modifiedIQueryable = _modifiedIQueryable.Include(GetSinglePropertyExpression<T, TKey>(propertyName)).ThenInclude(GetSinglePropertyExpression<TKey, TSecondKey>(thenIncludeProperty));
            }
        }
        /// <summary>
        /// Adds Take to the queryable
        /// </summary>
        /// <param name="take">How many results should we take</param>
        public void Take(int take)
        {
             _modifiedIQueryable = _modifiedIQueryable.Take(take);
        }
        /// <summary>
        /// Adds Skip to the queryable
        /// </summary>
        /// <param name="skip">How many results should we skip</param>
        public void Skip(int skip)
        {
            _modifiedIQueryable = _modifiedIQueryable.Skip(skip);
        }
        /// <summary>
        /// Runs the distinct method on queryable
        /// </summary>
        public void Distinct()
        {
            _modifiedIQueryable = _modifiedIQueryable.Distinct();
        }
        /// <summary>
        /// Adds the select property to a list to be applied when running the query
        /// </summary>
        /// <param name="names">Name of the property to select</param>
        public void Select(string[] names)
        {
            foreach(string name in names)
            {
                SelectProperties.Add(name);
            }
        }
        /// <summary>
        /// Gets an expression like this: var.Select(p => p.Name)
        /// </summary>
        /// <typeparam name="TKey">Type of the obj (p from example)</typeparam>
        /// <typeparam name="TKey2">Type of the property (name from example) </typeparam>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Correct expression</returns>
        internal static Expression<Func<TKey, TKey2>> GetSinglePropertyExpression<TKey, TKey2>(string propertyName)
        {
            var newParam = Expression.Parameter(typeof(TKey), "newP");
            var body = Expression.Property(newParam, propertyName);
            ParameterExpression[] array = new ParameterExpression[1];
            array[0] = newParam;
            return Expression.Lambda<Func<TKey, TKey2>>(body, array);
        }
        /// <summary>
        /// Run by reflection to apply OrderBy correctly
        /// </summary>
        /// <typeparam name="TKey">Type of the property we are ordering</typeparam>
        /// <param name="propertyName">Name of the property we're ordering on</param>
        internal void OrderByInternal<TKey>(string propertyName)
        {
            _modifiedIQueryable = _modifiedIQueryable.OrderBy(GetSinglePropertyExpression<T, TKey>(propertyName));
        }
        /// <summary>
        /// Run by reflection to apply OrderByDescending correctly
        /// </summary>
        /// <typeparam name="TKey">Type of the property we are ordering</typeparam>
        /// <param name="propertyName">Name of the property we're ordering on</param>
        internal void OrderByDescendingInternal<TKey>(string propertyName)
        {
            _modifiedIQueryable = _modifiedIQueryable.OrderByDescending(GetSinglePropertyExpression<T, TKey>(propertyName));
        }
        /// <summary>
        /// Runs the OrderbyInternal to apply OrderBy
        /// </summary>
        /// <param name="propertyName">Name of the property we are ordering</param>
        public void OrderBy(string propertyName)
        {
            var methodInfo = this.GetType().GetMethod("OrderByInternal", BindingFlags.NonPublic | BindingFlags.Instance);
            var propertyInfo = typeof(T).GetProperties().Where(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var gMethod = methodInfo.MakeGenericMethod(propertyInfo.PropertyType);
            gMethod.Invoke(this, new[] { propertyInfo.Name });
        }
        /// <summary>
        /// Runs the OrderbyDescendingInternal to apply OrderBy
        /// </summary>
        /// <param name="propertyName">Name of the property we are ordering</param>
        public void OrderByDescending(string propertyName)
        {
            var methodInfo = this.GetType().GetMethod("OrderByDescendingInternal", BindingFlags.NonPublic | BindingFlags.Instance);
            var propertyInfo = typeof(T).GetProperties().Where(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var gMethod = methodInfo.MakeGenericMethod(propertyInfo.PropertyType);
            gMethod.Invoke(this, new[] { propertyInfo.Name });
        }
        /// <summary>
        /// Returns the base type. Useful in PowerShell and reflection to get the base type
        /// </summary>
        /// <returns>T</returns>
        public Type GetBaseType()
        {
            return typeof(T);
        }
        /// <summary>
        /// Gets the queryable object - for advanced scenarios
        /// </summary>
        /// <returns>the IQueryable</returns>
        public IQueryable<T> GetQueryable()
        {
            return _baseIQueryable;
        }
        /// <summary>
        /// Applys a ScriptBlock expression to the queryable
        /// </summary>
        /// <param name="script">Script to apply</param>
        /// <param name="Arguments">Any arguments with the script</param>
        /// <param name="VariableValues">If any varaibles are in the script, attempt to get the values</param>
        public void ApplyExpression(ScriptBlock script, object[] Arguments = null, Dictionary<string, object> VariableValues = null)
        {
            var binaryConverter = new PoshBinaryConverter<T>();
            var expressionToApply = binaryConverter.ConvertBinaryExpression(script, Arguments, VariableValues);
            _modifiedIQueryable = _modifiedIQueryable.Where(expressionToApply);
        }
        /// <summary>
        /// Resets the queryable - if a search is set up wrong, this clears everything
        /// </summary>
        public void Reset()
        {
            _modifiedIQueryable = _baseIQueryable.AsQueryable();
            SelectProperties.Clear();
            if (!string.IsNullOrEmpty(_fromSql))
            {
                FromSql(_fromSql);
            }
        }

    }

}
