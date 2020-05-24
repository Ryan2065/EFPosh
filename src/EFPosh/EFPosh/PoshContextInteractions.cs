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
using Microsoft.Extensions.DependencyInjection;

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
        static IServiceProvider BuildServiceProvider(IServiceCollection services)
        {
            var t = typeof(ServiceCollectionContainerBuilderExtensions).GetTypeInfo();
            var BuildServiceProviderMethod = t.GetMethod(nameof(BuildServiceProvider), new Type[] { typeof(IServiceCollection), typeof(bool) });
            return (IServiceProvider)BuildServiceProviderMethod.Invoke(null, new object[] { services, false });
        }
        private DbContext _poshContext;
        public void NewDbContext<T>(
            string connectionString,
            string dbType,
            bool EnsureCreated,
            bool ReadOnly,
            PoshEntity[] Types = null,
            PoshEntityRelationship[] Relationships = null
        )
            where T : DbContext
        {
            var dbOptions = new DbContextOptionsBuilder<T>();
            IServiceCollection coll = new ServiceCollection();
            switch (dbType.ToUpper())
            {
                case "SQLITE":
                    dbOptions.UseSqlite(connectionString);
                    coll = coll.AddEntityFrameworkSqlite();
                    break;
                case "MSSQL":
                default:
                    dbOptions.UseSqlServer(connectionString);
                    coll = coll.AddEntityFrameworkSqlServer();
                    break;
            }
            var sp = BuildServiceProvider(coll);
            dbOptions.UseInternalServiceProvider(sp);
            DbContext dbContext = null;
            if(typeof(T) == typeof(PoshContext))
            {
                dbContext = new PoshContext(dbOptions.Options, Types, Relationships);
            }
            else
            {
                dbContext = (DbContext)Activator.CreateInstance(typeof(T), new object[] { dbOptions.Options });
            }
            if (EnsureCreated)
            {
                dbContext.Database.EnsureCreated();
            }
            if (ReadOnly)
            {
                dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
            _poshContext = dbContext;
        }
        public void ExistingContext(
            string connectionString,
            string dbType,
            bool EnsureCreated,
            bool ReadOnly,
            string dllPath,
            string ContextClassName
        )
        {
            var assembly = Assembly.LoadFile(dllPath);
            var type = assembly.GetTypes().Where(p => p.Name.ToLower().Equals(ContextClassName.ToLower()) ).FirstOrDefault();
            PoshEntityRelationship[] Relationships = null;
            PoshEntity[] Types = null;
            typeof(PoshContextInteractions)
                    .GetMethod("NewDbContext")
                    .MakeGenericMethod(type)
                    .Invoke(this, new object[] { connectionString, dbType, EnsureCreated, ReadOnly, Types, Relationships });
            
        }
        public void NewPoshContext(
            string connectionString,
            string dbType,
            PoshEntity[] Types,
            bool EnsureCreated,
            bool ReadOnly
        )
        {

            PoshEntityRelationship[] Relationships = null;
            NewDbContext<PoshContext>(connectionString, dbType, EnsureCreated, ReadOnly, Types, Relationships);
        }
        
        public DbContext DBContext
        {
            get { return _poshContext; }
            set { _poshContext = value; }
        }

        public PoshEntityColumn<T> NewQuery<T>()
            where T : class
        {
            return new PoshEntityColumn<T>(_poshContext, "", new List<object>(), "", new List<object>() );
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
            var typeList = _poshContext.Model
                                .GetEntityTypes()
                                .Select(p => p.ClrType)
                                .ToList();
            var requestedType = typeList.Where(p => p.Name.ToUpper().Equals(binder.Name.ToUpper())).FirstOrDefault();
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
        public List<string> GetEntities()
        {
            return _poshContext.Model.GetEntityTypes().Select(p => p.ClrType).Select(d => d.Name).ToList();
        }
    }
    public class PoshContextEntity<T> where T : class
    {
        public PoshContextEntity(DbContext context)
        {
            _baseIQueryable = context.Set<T>().AsQueryable();
        }
        private IQueryable<T> _baseIQueryable;
        public void AsNoTracking()
        {
            _baseIQueryable = _baseIQueryable.AsNoTracking();
        }
        public PoshContextEntity<T> WhereQuery(string whereQuery, object[] QueryObjects)
        {
            _baseIQueryable = _baseIQueryable.Where(whereQuery, QueryObjects);
            return this;
        }
        public PoshContextEntity<T> Take(int take)
        {
            _baseIQueryable = _baseIQueryable.Take(take);
            return this;
        }

        public T New()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        public PoshContextEntity<T> Skip(int skip)
        {
            _baseIQueryable = _baseIQueryable.Skip(skip);
            return this;
        }

        public PoshContextEntity<T> OrderBy(string orderBy)
        {
            _baseIQueryable = _baseIQueryable.OrderBy(orderBy);
            return this;
        }

        public List<T> ToList()
        {
            return _baseIQueryable.ToList();
        }
        public T FirstOrDefault()
        {
            return _baseIQueryable.FirstOrDefault();
        }
    }
}
