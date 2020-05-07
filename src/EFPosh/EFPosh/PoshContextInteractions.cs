using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore.Internal;

namespace EFPosh
{
    public class PoshContextInteractions
    {
        private DbContext _poshContext;
        private IQueryable<object> _baseIQueryable;
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
        
        public void NewIQueryable(Type type)
        {
            _baseIQueryable = (IQueryable<object>)typeof(DbContext)
                .GetMethod("Set")
                .MakeGenericMethod(type)
                .Invoke(_poshContext, new object[] { });
        }

        private void CheckIfIQueryableEmpty()
        {
            if(_baseIQueryable == null)
            {
                throw new Exception("IQueryable not created. Must first execute .NewIQueryable(type) or NewIQueryable did not complete successfully");
            }
        }

        public void Where(string whereQuery, object[] QueryObjects)
        {
            CheckIfIQueryableEmpty();
            _baseIQueryable = _baseIQueryable.Where(whereQuery, QueryObjects);
        }

        public void Take(int take)
        {
            CheckIfIQueryableEmpty();
            _baseIQueryable = _baseIQueryable.Take(take);
        }

        public void Skip(int skip)
        {
            CheckIfIQueryableEmpty();
            _baseIQueryable = _baseIQueryable.Skip(skip);
        }

        public void OrderBy(string orderBy)
        {
            CheckIfIQueryableEmpty();
            _baseIQueryable = _baseIQueryable.OrderBy(orderBy);
        }

        public List<object> ToList()
        {
            CheckIfIQueryableEmpty();
            var tempIqueryable = _baseIQueryable;
            ClearIQueryable();
            return tempIqueryable.ToList();
        }

        public object FirstOrDefault()
        {
            CheckIfIQueryableEmpty();
            var tempIqueryable = _baseIQueryable;
            ClearIQueryable();
            return tempIqueryable.FirstOrDefault();
        }

        public void Add(object obj)
        {
            _poshContext.Add(obj);
            _poshContext.SaveChanges();
        }

        public void AddRange(object[] objs)
        {
            _poshContext.AddRange(objs);
            _poshContext.SaveChanges();
        }

        public void SaveChanges()
        {
            _poshContext.SaveChanges();
        }

        public void ClearIQueryable()
        {
            _baseIQueryable = null;
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
}
