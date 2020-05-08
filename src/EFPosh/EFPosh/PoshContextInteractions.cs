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

namespace EFPosh
{
    public class PoshContextInteractions
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
                
        public void Add(object obj)
        {
            _poshContext.Add(obj);
        }

        public void AddRange(object[] objs)
        {
            _poshContext.AddRange(objs);
        }

        public void SaveChanges()
        {
            _poshContext.SaveChanges();
        }

        public void Remove(object obj)
        {
            _poshContext.Remove(obj);
        }

        public void RemoveRange(object[] objs)
        {
            _poshContext.RemoveRange(objs);
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
        public void Where(string whereQuery, object[] QueryObjects)
        {
            _baseIQueryable = _baseIQueryable.Where(whereQuery, QueryObjects);
        }
        public void Take(int take)
        {
            _baseIQueryable = _baseIQueryable.Take(take);
        }

        public void Skip(int skip)
        {
            _baseIQueryable = _baseIQueryable.Skip(skip);
        }

        public void OrderBy(string orderBy)
        {
            _baseIQueryable = _baseIQueryable.OrderBy(orderBy);
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
