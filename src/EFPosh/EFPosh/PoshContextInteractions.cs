using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Dynamic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFPosh
{
    /// <summary>
    /// Allows PowerShell to interact with the DbContext. Directly interacting with the DbContext will cause errors due to assemblies not being fully loaded
    /// Calling through here will allow the assembly loaders to find missing assemblies on demand so DbContext interactions won't fail.
    /// </summary>
    public class PoshContextInteractions : DynamicObject
    {
        /// <summary>
        /// Logger to log information to PowerShell using streams
        /// </summary>
        private PoshILogger _logger;
        /// <summary>
        /// A list of entities marked as "FromSql", this means we don't directly query the Db, but instead sub in a sql script for the query
        /// </summary>
        private Dictionary<string, string> FromSqlEntities;
        /// <summary>
        /// DbContext this class is interacting with
        /// </summary>
        private DbContext _poshContext;
        /// <summary>
        /// Default constructor - Will set up PowerShell logging and the assembly loaders
        /// </summary>
        public PoshContextInteractions()
        {
            _logger = new PoshILogger(LogLevel.Trace);
            AppDomain currentDomain = AppDomain.CurrentDomain;
            try
            {

            currentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolvers.PoshResolveEventHandler);
#if NET6_0
            NativeLibrary.SetDllImportResolver(typeof(SQLitePCL.SQLite3Provider_e_sqlite3).Assembly, AssemblyResolvers.NativeAssemblyResolver);
#endif
            }
            catch(Exception ex)
            {
                _logger.LogDebug($"Could not load assembly resolver - Error {ex.Message}");
            }
            FromSqlEntities = new Dictionary<string, string>();
        }
#if NET6_0_OR_GREATER
        public static readonly ILoggerFactory PoshLoggerFactory = LoggerFactory.Create(builder => { 
                                                                    builder.SetMinimumLevel(LogLevel.Warning)
                                                                            .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information)
                                                                            .AddPoshLogger(config =>
                                                                            {
                                                                                config.LogLevelStreamMappings.Add(LogLevel.Information, PoshLogStream.Debug);
                                                                                config.LogLevelStreamMappings.Add(LogLevel.None, PoshLogStream.Debug);
                                                                            });
                                                                          });
#endif
        public void SetDependencyFolder(string dependencyFolder)
        {
            if(!AssemblyResolvers.dllPathsToCheck.Any(p => p == dependencyFolder))
            {
                AssemblyResolvers.dllPathsToCheck.Add(dependencyFolder);
            }
        }
        
        static IServiceProvider BuildServiceProvider(IServiceCollection services)
        {
            services.AddLogging(p => p.SetMinimumLevel(LogLevel.Warning).AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Trace).AddPoshLogger(config =>
            {
                config.LogLevelStreamMappings.Add(LogLevel.Information, PoshLogStream.Debug);
                config.LogLevelStreamMappings.Add(LogLevel.None, PoshLogStream.Debug);
            }));
            var t = typeof(ServiceCollectionContainerBuilderExtensions).GetTypeInfo();
            var BuildServiceProviderMethod = t.GetMethod(nameof(BuildServiceProvider), new Type[] { typeof(IServiceCollection), typeof(bool) });
            return (IServiceProvider)BuildServiceProviderMethod.Invoke(null, new object[] { services, false });
        }
        
        public void NewDbContext<T>(
            string connectionString,
            string dbType,
            bool EnsureCreated,
            bool RunMigrations,
            bool ReadOnly,
            PoshEntity[] Types = null
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
#if NET6_0_OR_GREATER
            dbOptions.UseLoggerFactory(PoshLoggerFactory);
#else
            var sp = BuildServiceProvider(coll);
            dbOptions.UseInternalServiceProvider(sp);
#endif


            DbContext dbContext = null;
            if(typeof(T) == typeof(PoshContext))
            {
                dbContext = new PoshContext(dbOptions.Options, Types);
            }
            else
            {
                _logger.LogDebug($"Creating new instance of {typeof(T).Name}");
                dbContext = (DbContext)Activator.CreateInstance(typeof(T), new object[] { dbOptions.Options });
            }
            if (RunMigrations)
            {
                _logger.LogDebug("Running Migrate - If the db is not on the newest version, it will be updated");
                dbContext.Database.Migrate();
            }
            else if (EnsureCreated)
            {
                _logger.LogDebug("Running EnsureCreated - If the db is not created yet, it will be created now.");
                dbContext.Database.EnsureCreated();
            }
            if (ReadOnly)
            {
                dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
            _poshContext = dbContext;
            if(Types == null) { return; }
            foreach (var t in Types)
            {
                if (!String.IsNullOrEmpty(t.FromSql))
                {
                    FromSqlEntities.Add(t.Type.Name, t.FromSql);
                }
            }
        }
        public void ExistingContext(
            string connectionString,
            string dbType,
            bool EnsureCreated,
            bool RunMigrations,
            bool ReadOnly,
            string dllPath,
            string ContextClassName
        )
        {
            _logger.LogDebug($"Attempting to load an exsiting DbContext at {dllPath}");
            var assembly = Assembly.LoadFile(dllPath);
            var type = assembly.GetTypes().Where(p => p.Name.ToLower().Equals(ContextClassName.ToLower())).FirstOrDefault();
            PoshEntity[] Types = null;
            typeof(PoshContextInteractions)
                    .GetMethod("NewDbContext")
                    .MakeGenericMethod(type)
                    .Invoke(this, new object[] { connectionString, dbType, EnsureCreated, RunMigrations, ReadOnly, Types });
        }
        public void NewPoshContext(
            string connectionString,
            string dbType,
            PoshEntity[] Types,
            bool EnsureCreated,
            bool RunMigrations,
            bool ReadOnly
        )
        {
            NewDbContext<PoshContext>(connectionString, dbType, EnsureCreated, RunMigrations, ReadOnly, Types);
        }
        
        public DbContext DBContext
        {
            get { return _poshContext; }
            set { _poshContext = value; }
        }

        public PoshEntityColumn<T> NewQuery<T>()
            where T : class
        {
            var returnObject = new PoshEntityColumn<T>(_poshContext);
            if (FromSqlEntities.ContainsKey(typeof(T).Name))
            {
                returnObject.FromSql(FromSqlEntities[typeof(T).Name]);
            }
            return returnObject;
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

        private Dictionary<string, object> CachedEntities { get; set; } = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder,
                                  out object result)
        {
            if (!CachedEntities.ContainsKey(binder.Name.ToUpper()))
            {
                
                var typeList = _poshContext.Model
                                .GetEntityTypes()
                                .Select(p => p.ClrType)
                                .ToList();
                var requestedType = typeList.Where(p => p.Name.ToUpper().Equals(binder.Name.ToUpper())).FirstOrDefault();
                result = null;
                if (requestedType != null)
                {
                    CachedEntities[binder.Name.ToUpper()] = typeof(PoshContextInteractions)
                        .GetMethod("NewQuery")
                        .MakeGenericMethod(requestedType)
                        .Invoke(this, null);

                }
            }
            try
            {
                result = CachedEntities[binder.Name.ToUpper()];
            }
            catch
            {
                result = null;
                // property not found - just return false below, result will be null
            }
            return result == null ? false : true;
        }
        public IEnumerable<string> GetEntities()
        {
            foreach (var prop in _poshContext.GetType().GetProperties())
            {
                if (typeof(IQueryable).IsAssignableFrom(prop.PropertyType))
                {
                    yield return prop.Name;
                }
            }
        }
    }
    
}
