using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EFPosh
{
    /// <summary>
    /// Base DbContext that will add entities on the fly based on an array of types
    /// </summary>
    public class PoshContext : DbContext
    {
        private readonly PoshEntity[] _types;
        public PoshContext(DbContextOptions options, PoshEntity[] Types) : base(options)
        {
            _types = Types;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        public void SetPrimaryKeys<T>(ModelBuilder modelBuilder, string[] Primarykeys)
            where T : class
        {
            modelBuilder.Entity<T>().HasKey(Primarykeys);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var t in _types)
            {
                if (t.Keyless)
                {
#if NETFRAMEWORK
                    modelBuilder.Query(t.Type);
#else
                    modelBuilder.Entity(t.Type).HasNoKey();
#endif
                }
                else
                {
                    modelBuilder.Entity(t.Type);
                }
                if (t.PrimaryKeys != null)
                {
                    var methodInfo = typeof(PoshContext).GetMethods().Where(p => p.Name.Equals("SetPrimaryKeys")).FirstOrDefault();
                    var genMethod = methodInfo.MakeGenericMethod(new Type[] { t.Type });
                    genMethod.Invoke(this, new object[] { modelBuilder, t.PrimaryKeys });
                }
                string tableName = string.IsNullOrEmpty(t.TableName) ? t.Type.Name : t.TableName;
                string schema = string.IsNullOrEmpty(t.Schema) ? null : t.Schema;
                if (t.Keyless)
                {
#if NETFRAMEWORK
                    modelBuilder.Query(t.Type).ToView(tableName, schema);
#else
                    modelBuilder.Entity(t.Type).ToView(tableName, schema);
#endif
                }
                else
                {
                    modelBuilder.Entity(t.Type).ToTable(tableName, schema);
                }
            }
        }
    }


}
