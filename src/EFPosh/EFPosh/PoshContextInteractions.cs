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
#if NET6_0
using Microsoft.Extensions.DependencyModel;
using System.Runtime.Loader;
using System.Text.Json.Nodes;
#endif

namespace EFPosh
{
    public class PoshContextInteractions : DynamicObject
    {
        private static List<string> dllPathsToCheck;
        private Dictionary<string, string> FromSqlEntities;
        public PoshContextInteractions()
        {

            AppDomain currentDomain = AppDomain.CurrentDomain;
            dllPathsToCheck = new List<string>();
            FindFoldersToCheckForDlls();
            currentDomain.AssemblyResolve += new ResolveEventHandler(PoshResolveEventHandler);
#if NET6_0
            NativeLibrary.SetDllImportResolver(typeof(SQLitePCL.SQLite3Provider_e_sqlite3).Assembly, NativeAssemblyResolver);
#endif
            FromSqlEntities = new Dictionary<string, string>();
        }
#if NET6_0
        static IntPtr NativeAssemblyResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (!libraryName.Equals("e_sqlite3", StringComparison.OrdinalIgnoreCase))
                return IntPtr.Zero;
            var currentFolder = Path.GetDirectoryName(typeof(PoshContextInteractions).Assembly.Location);
            IntPtr libHandle = IntPtr.Zero;
            string dllPath = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                dllPath = Path.Combine(currentFolder, "runtimes", "win-x64", "native", "e_sqlite3.dll");
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.X86)
            {
                dllPath = Path.Combine(currentFolder, "runtimes", "win-x86", "native", "e_sqlite3.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                dllPath = Path.Combine(currentFolder, "runtimes", "linux-x64", "native", "libe_sqlite3.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                dllPath = Path.Combine(currentFolder, "runtimes", "osx", "native", "libe_sqlite3.dylib");
            }
            if (File.Exists(dllPath))
            {
                NativeLibrary.TryLoad(dllPath, assembly, searchPath, out libHandle);
            }
            return libHandle;
        }
#endif

        private static void FindFoldersToCheckForDlls()
        {
            var assemblyFolder = Path.GetDirectoryName(typeof(PoshContextInteractions).Assembly.Location);
            dllPathsToCheck.Add(assemblyFolder);
        }

        private static Assembly PoshResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var dllNeeded = args.Name.Split(',')[0] + ".dll";
            foreach(var directoryInfo in dllPathsToCheck)
            {
                var fullDLLPath = Path.Combine(directoryInfo, dllNeeded);
                if (File.Exists(fullDLLPath))
                {
                    return Assembly.LoadFrom(fullDLLPath);
                }
            }
            return null;
        }

        static IServiceProvider BuildServiceProvider(IServiceCollection services)
        {
            services.AddLogging(p => p.SetMinimumLevel(LogLevel.Warning).AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information).AddPoshLogger(config =>
            {
                config.LogLevelStreamMappings.Add(LogLevel.Information, PoshLogStream.Debug);
                config.LogLevelStreamMappings.Add(LogLevel.None, PoshLogStream.Debug);
            }));
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
            var sp = BuildServiceProvider(coll);
            dbOptions.UseInternalServiceProvider(sp);
            
            DbContext dbContext = null;
            if(typeof(T) == typeof(PoshContext))
            {
                dbContext = new PoshContext(dbOptions.Options, Types);
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
            foreach(var t in Types)
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
            bool ReadOnly,
            string dllPath,
            string ContextClassName
        )
        {
            var assembly = Assembly.LoadFile(dllPath);
            dllPathsToCheck.Add(Path.GetDirectoryName(assembly.Location));
            var type = assembly.GetTypes().Where(p => p.Name.ToLower().Equals(ContextClassName.ToLower()) ).FirstOrDefault();
            PoshEntity[] Types = null;
            typeof(PoshContextInteractions)
                    .GetMethod("NewDbContext")
                    .MakeGenericMethod(type)
                    .Invoke(this, new object[] { connectionString, dbType, EnsureCreated, ReadOnly, Types });
            
        }
        public void NewPoshContext(
            string connectionString,
            string dbType,
            PoshEntity[] Types,
            bool EnsureCreated,
            bool ReadOnly
        )
        {
            NewDbContext<PoshContext>(connectionString, dbType, EnsureCreated, ReadOnly, Types);
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
        public List<string> GetEntities()
        {
            return _poshContext.Model.GetEntityTypes().Select(p => p.ClrType).Select(d => d.Name).ToList();
        }
    }
    
}
