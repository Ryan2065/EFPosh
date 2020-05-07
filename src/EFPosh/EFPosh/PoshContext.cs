using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace EFPosh
{
    public class PoshContext : DbContext
    {
        private PoshEntity[] _types;
        public PoshContext(DbContextOptions options, PoshEntity[] Types) : base(options)
        {
            _types = Types;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityMethod = typeof(ModelBuilder).GetMethods().Single(
                                    m => m.Name.Equals("Entity") &&
                                    m.IsGenericMethod == true &&
                                    m.GetParameters().Count() == 0
                                );
            var queryMethod = typeof(ModelBuilder).GetMethods().Single(
                                    m => m.Name.Equals("Query") &&
                                    m.IsGenericMethod == true &&
                                    m.GetParameters().Count() == 0
                                );
            foreach (var t in _types)
            {
                if (t.Keyless)
                {
                    modelBuilder.Query(t.Type);
                    if (!string.IsNullOrEmpty(t.TableName))
                    {
                        if (!string.IsNullOrEmpty(t.Schema))
                        {
                            modelBuilder.Query(t.Type).ToView(t.TableName, t.Schema);
                        }
                        else
                        {
                            modelBuilder.Query(t.Type).ToView(t.TableName);
                        }
                    }
                    else if (!string.IsNullOrEmpty(t.Schema))
                    {
                        modelBuilder.Query(t.Type).ToView(t.Type.GetType().Name, t.Schema);
                    }
                    //queryMethod.MakeGenericMethod(t.Type).Invoke(modelBuilder, new object[] { });
                }
                else
                {
                    //entityMethod.MakeGenericMethod(t.Type).Invoke(modelBuilder, new object[] { });
                    modelBuilder.Entity(t.Type);
                    if (t.PrimaryKeys != null)
                    {
                        modelBuilder.Entity(t.Type).HasKey(t.PrimaryKeys);
                    }
                    if (!string.IsNullOrEmpty(t.TableName))
                    {
                        if (!string.IsNullOrEmpty(t.Schema))
                        {
                            modelBuilder.Entity(t.Type).ToTable(t.TableName, t.Schema);
                        }
                        else
                        {
                            modelBuilder.Entity(t.Type).ToTable(t.TableName);
                        }
                    }
                    else if (!string.IsNullOrEmpty(t.Schema))
                    {
                        modelBuilder.Entity(t.Type).ToTable(t.Type.GetType().Name, t.Schema);
                    }
                }
            }
        }
    }
}
