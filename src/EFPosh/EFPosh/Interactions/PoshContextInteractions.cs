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
using PoshLogger;

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
        private readonly PoshILogger _logger;
        /// <summary>
        /// A list of entities marked as "FromSql", this means we don't directly query the Db, but instead sub in a sql script for the query
        /// </summary>
        private readonly Dictionary<string, string> FromSqlEntities;
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
                if (!EFPosh.AssemblyResolvers.LoadedSqliteResolver)
                {
                    currentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolvers.PoshResolveEventHandler);
#if NET6_0
                    NativeLibrary.SetDllImportResolver(typeof(SQLitePCL.SQLite3Provider_e_sqlite3).Assembly, AssemblyResolvers.NativeAssemblyResolver);
#endif
                    EFPosh.AssemblyResolvers.LoadedSqliteResolver = true;
                }

            }
            catch(Exception ex)
            {
                _logger.LogWarning("Could not load assembly resolver - Error {ExceptionMessage}", ex.Message);
            }
            FromSqlEntities = new Dictionary<string, string>();
        }
#if NET6_0_OR_GREATER
        /// <summary>
        /// Used in .net 6+ to set up the ILogger for the new way Entity Framework needs loggers
        /// </summary>
        public static readonly ILoggerFactory PoshLoggerFactory = LoggerFactory.Create(builder => { 
                                                                    builder.SetMinimumLevel(LogLevel.Warning)
                                                                            .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information)
                                                                            .AddPoshLogger(config =>
                                                                            {
                                                                                config.Level = LogLevel.Trace;
                                                                                config.LevelMappings.Add(LogLevel.Information, PoshLogLevel.Verbose);
                                                                            });
                                                                          });
#endif
        /// <summary>
        /// Able to be called by PowerShell to manually set the dependency folder if different from executing assembly.
        /// Useful in debugging
        /// </summary>
        /// <param name="dependencyFolder">Folder path where the dependencies are located</param>
        public void SetDependencyFolder(string dependencyFolder)
        {
            if(!AssemblyResolvers.DllPathsToCheck.Any(p => p == dependencyFolder))
            {
                AssemblyResolvers.DllPathsToCheck.Add(dependencyFolder);
            }
        }
        /// <summary>
        /// In Entity Framework 2, a service provider is required so this method sets that up
        /// </summary>
        /// <param name="services">Collection of services to build into the provider</param>
        /// <returns>Built provider</returns>
        static IServiceProvider BuildServiceProvider(IServiceCollection services)
        {
            services.AddLogging(p => p.SetMinimumLevel(LogLevel.Warning).AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Trace).AddPoshLogger(config =>
            {
                config.Level = LogLevel.Trace;
                config.LevelMappings.Add(LogLevel.Information, PoshLogLevel.Verbose);
            }));
            var t = typeof(ServiceCollectionContainerBuilderExtensions).GetTypeInfo();
            var BuildServiceProviderMethod = t.GetMethod(nameof(BuildServiceProvider), new Type[] { typeof(IServiceCollection), typeof(bool) });
            return (IServiceProvider)BuildServiceProviderMethod.Invoke(null, new object[] { services, false });
        }
        /// <summary>
        /// Creates the Entity Framework context
        /// </summary>
        /// <typeparam name="T">Type of the DbContext</typeparam>
        /// <param name="connectionString">Connection string that will be used in the DbOptions</param>
        /// <param name="dbType">What type of DB is this? Current values can be MSSQL or SQLite</param>
        /// <param name="EnsureCreated">Should we create the DB if it isn't created?</param>
        /// <param name="RunMigrations">Should we run migrations if they are required?</param>
        /// <param name="ReadOnly">Will disable the change tracker in the Db to make it faster and read only</param>
        /// <param name="Types">If this is a PoshContext Db, types are needed to create the object</param>
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
            ConnectionString = connectionString;
            _poshContext = null;
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
#if NET6_0_OR_GREATER
                    // c# can't figure out what assembly of Microsoft.Data.SqlClient to load, so we must help by pre-loading it and setting up a resolver for the native library
                    // it defaults to .netstandard2.0 which won't work in Posh7.
                    AssemblyResolvers.LoadSqlClient();
#endif
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
                _logger.LogDebug("Creating new PoshContext");
                dbContext = new PoshContext(dbOptions.Options, Types);
            }
            else
            {
                _logger.LogDebug("Creating new instance of {Type}", typeof(T).Name);
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
            DBContext = dbContext;
            if (Types == null) { return; }
            foreach (var t in Types)
            {
                if (!String.IsNullOrEmpty(t.FromSql))
                {
                    FromSqlEntities.Add(t.Type.Name, t.FromSql);
                }
            }
        }
        /// <summary>
        /// Called if someone is using a context already compiled in a dll
        /// </summary>
        /// <param name="connectionString">Connection string for the db</param>
        /// <param name="dbType">What type of DB are we connecting to</param>
        /// <param name="EnsureCreated">Do we run the ensure created method?</param>
        /// <param name="RunMigrations">Do we run the run migrations method?</param>
        /// <param name="ReadOnly">True will create the db context and disable the change tracker</param>
        /// <param name="dllPath">Path to assembly where the context is. This needs to be loaded in here with the assembly resolvers
        ///                       so will need to be provided</param>
        /// <param name="ContextClassName">Class of the DbContext</param>
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
            _logger.LogDebug("Attempting to load an exsiting DbContext at {dllPath}", dllPath);
            var assembly = Assembly.LoadFile(dllPath);
            var type = assembly.GetTypes().Where(p => p.Name.ToLower().Equals(ContextClassName.ToLower())).FirstOrDefault();
            PoshEntity[] Types = null;
            typeof(PoshContextInteractions)
                    .GetMethod("NewDbContext")
                    .MakeGenericMethod(type)
                    .Invoke(this, new object[] { connectionString, dbType, EnsureCreated, RunMigrations, ReadOnly, Types });
        }
        /// <summary>
        /// Creates a new DbContext based off classes defined in PowerShell
        /// </summary>
        /// <param name="connectionString">Connection string of the db we're connecting to</param>
        /// <param name="dbType">What type of Db are we conencting to</param>
        /// <param name="Types">Array of types to add to the Posh db context</param>
        /// <param name="EnsureCreated">Do we want to make sure it is created?</param>
        /// <param name="RunMigrations">Do we want to run any migrations stored in the context</param>
        /// <param name="ReadOnly">Is this a read only db</param>
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
        
        /// <summary>
        /// Public accessor for the DbContext - useful for users who want to inspect it or do advanced things
        /// </summary>
        public DbContext DBContext
        {
            get { return _poshContext; }
            set { _poshContext = value; }
        }
        /// <summary>
        /// Called with reflection from TryGetMember - Will create a new PoshEntityInteraction class
        /// </summary>
        /// <typeparam name="T">Type of the Entity we're interacting with</typeparam>
        /// <returns>Returns the PoshEntityInteraction</returns>
        public PoshEntityInteractions<T> NewPoshEntityInteraction<T>()
            where T : class
        {
            var returnObject = new PoshEntityInteractions<T>(_poshContext);
            if (FromSqlEntities.ContainsKey(typeof(T).Name))
            {
                returnObject.FromSql(FromSqlEntities[typeof(T).Name], true);
            }
            return returnObject;
        }
        /// <summary>
        /// Sometimes PowerShell classes are duplicated. If that happens, someone might send us a correct class name, but
        /// not the correct class type. This method attempts to fix this by finding the correct type in the Database context
        /// and changing the object to that.
        /// </summary>
        /// <param name="obj">Object we are checking</param>
        /// <returns>Hopefully the correct type</returns>
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
        /// <summary>
        /// Used to add an entity to the database. Will correct the type if the entity does not have a correct type
        /// </summary>
        /// <param name="obj">Object to add to the db</param>
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
        /// <summary>
        /// Adds a range of objects to the db. Will correct the types if they aren't correct.
        /// </summary>
        /// <param name="objs">Object to add to the db</param>
        public void AddRange(object[] objs)
        {
            try
            {
                _poshContext.AddRange(objs);
            }
            catch(InvalidCastException)
            {
                List<object> NewObjectList = new();
                foreach(var instance in objs)
                {
                    NewObjectList.Add(ConvertType(instance));
                }
                _poshContext.AddRange(NewObjectList);
            }
        }
        /// <summary>
        /// Call SaveChanges on the db context
        /// </summary>
        public void SaveChanges()
        {
            _poshContext.SaveChanges();
        }
        /// <summary>
        /// Will remove an object from the db. This will correct types if they are wrong
        /// </summary>
        /// <param name="obj">Object to remove from the db</param>
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
        /// <summary>
        /// Will remove a range of objects from the db. Will correct types if they are wrong
        /// </summary>
        /// <param name="objs">Objects to remove from the db</param>
        public void RemoveRange(object[] objs)
        {
            try
            {
                _poshContext.RemoveRange(objs);
            }
            catch (InvalidCastException)
            {
                List<object> NewObjectList = new();
                foreach (var instance in objs)
                {
                    NewObjectList.Add(ConvertType(instance));
                }
                _poshContext.RemoveRange(NewObjectList);
            }
        }
        /// <summary>
        /// Used in TryGetMember - Didn't want a new object created every time someone called $db.EntityName
        /// </summary>
        private Dictionary<string, object> CachedEntities { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// Dynamic object requirement - this method will make all Entities accessible on the dbcontext
        /// </summary>
        /// <param name="binder">Default param</param>
        /// <param name="result">Default param</param>
        /// <returns>If it found something or not</returns>
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
                        .GetMethod("NewPoshEntityInteraction")
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
            return result != null;
        }
        /// <summary>
        /// Returns a list of entites in the Db Context - used in Posh to validate entities are correct
        /// </summary>
        /// <returns>List of entities</returns>
        public IEnumerable<string> GetEntities()
        {
            var typeList = _poshContext.Model
                .GetEntityTypes()
                .Select(p => p.ClrType)
                .ToList();
            foreach(var item in typeList)
            {
                yield return item.Name;
            }
        }
        /// <summary>
        /// Connection string used to create the Db. Useful when someone wants to re-create the Db
        /// </summary>
        public string ConnectionString { get; set; }

    }
    
}
