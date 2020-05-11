using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using Microsoft.EntityFrameworkCore.Internal;
using System.Reflection;
using System.IO;
using System.Linq.Expressions;
using System.Dynamic;

namespace EFPosh
{
    public class PoshContextInteractions : DynamicObject
    {
        public PoshContextInteractions()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(PoshResolveEventHandler);
        }
        private static Assembly PoshResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var dllNeeded = args.Name.Split(',')[0] + ".dll";
            var directoryInfo = Path.GetDirectoryName(typeof(PoshContextInteractions).Assembly.Location);
            var fullDLLPath = Path.Combine(directoryInfo, dllNeeded);
            if (File.Exists(fullDLLPath))
            {
                return Assembly.LoadFrom(fullDLLPath);
            }
            return null;
        }
        private DbContext _poshContext;
        public void NewPoshContext(
            string connectionString,
            string dbType,
            PoshEntity[] Types,
            bool EnsureCreated
        )
        {
            var dbOptions = new DbContextOptionsBuilder<PoshContext>();
            switch (dbType.ToUpper())
            {
                case "SQLITE":
                    dbOptions.UseSqlite(connectionString);
                    break;
                case "MSSQL":
                default:
                    dbOptions.UseSqlServer(connectionString);
                    break;
            }
            var dbContext = new PoshContext(dbOptions.Options, Types);
            if (EnsureCreated)
            {
                dbContext.Database.EnsureCreated();
            }
            _poshContext = dbContext;
        }
        
        public DbContext DBContext
        {
            get { return _poshContext; }
            set { _poshContext = value; }
        }

        public PoshContextQuery<T> NewQuery<T>()
            where T : class
        {
            return new PoshContextQuery<T>(_poshContext);
        }
        
        private object ConvertType(object obj)
        {
            var objectType = obj.GetType();
            Type newType = null;
            var ets = _poshContext.Model.GetEntityTypes();
            foreach (var et in ets)
            {
                if (et.Name.Equals(objectType.Name))
                {
                    newType = et.ClrType;
                }
            }
            if(newType != null)
            {
                var newObj = Activator.CreateInstance(newType);
                foreach (var property in newType.GetProperties())
                {
                    property.SetValue(newObj, obj.GetType().GetProperty(property.Name).GetValue(obj));
                }
                return newObj;
            }
            return obj;
        }
                
        public void Add(object obj)
        {
            try
            {
                _poshContext.Add(obj);
            }
            catch (InvalidCastException)
            {
                _poshContext.Add(ConvertType(obj));
            }
        }

        public void AddRange(object[] objs)
        {
            try
            {
                _poshContext.AddRange(objs);
            }
            catch(InvalidCastException)
            {
                List<object> NewObjectList = new List<object>();
                foreach(var instance in objs)
                {
                    NewObjectList.Add(ConvertType(instance));
                }
                _poshContext.AddRange(NewObjectList);
            }
        }

        public void SaveChanges()
        {
            _poshContext.SaveChanges();
        }

        public void Remove(object obj)
        {
            try
            {
                _poshContext.Remove(obj);
            }
            catch (InvalidCastException)
            {
                _poshContext.Remove(ConvertType(obj));
            }
        }

        public void RemoveRange(object[] objs)
        {
            try
            {
                _poshContext.RemoveRange(objs);
            }
            catch (InvalidCastException)
            {
                List<object> NewObjectList = new List<object>();
                foreach (var instance in objs)
                {
                    NewObjectList.Add(ConvertType(instance));
                }
                _poshContext.RemoveRange(NewObjectList);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder,
                                  out object result)
        {
            var requestedType = _poshContext.Model
                                .GetEntityTypes()
                                .Where(p => p.Name.Equals(binder.Name))
                                .Select(p => p.ClrType)
                                .FirstOrDefault();
            result = null;
            if(requestedType != null)
            {
                result = typeof(PoshContextInteractions)
                    .GetMethod("NewQuery")
                    .MakeGenericMethod(requestedType)
                    .Invoke(this, null);
            }
            return result == null ? false : true;
        }

        public Dictionary<string, List<string>> GetTableProperties()
        {
            var returnDict = new Dictionary<string, List<string>>();
            var tableList = _poshContext.Model.GetEntityTypes();
            foreach(var table in tableList)
            {
                returnDict.Add(table.Name, table.GetProperties().Select(p => p.Name).ToList());
            }
            return returnDict;
        }
    }
    public class PoshContextQuery<T> where T : class
    {
        public PoshContextQuery(DbContext context)
        {
            _baseIQueryable = context.Set<T>().AsQueryable();
        }
        private IQueryable<T> _baseIQueryable;
        public void AsNoTracking()
        {
            _baseIQueryable = _baseIQueryable.AsNoTracking();
        }
        public PoshContextQuery<T> Where(string whereQuery, object[] QueryObjects)
        {
            _baseIQueryable = _baseIQueryable.Where(whereQuery, QueryObjects);
            return this;
        }
        public PoshContextQuery<T> Take(int take)
        {
            _baseIQueryable = _baseIQueryable.Take(take);
            return this;
        }

        public PoshContextQuery<T> Skip(int skip)
        {
            _baseIQueryable = _baseIQueryable.Skip(skip);
            return this;
        }

        public PoshContextQuery<T> OrderBy(string orderBy)
        {
            _baseIQueryable = _baseIQueryable.OrderBy(orderBy);
            return this;
        }

        public List<T> ToList()
        {
            return _baseIQueryable.ToList();
        }
        public object FirstOrDefault()
        {
            return _baseIQueryable.FirstOrDefault();
        }

    }
}
