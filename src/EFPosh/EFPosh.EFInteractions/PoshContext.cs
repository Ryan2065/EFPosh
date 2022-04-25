using Microsoft.EntityFrameworkCore;

namespace EFPosh
{
    /// <summary>
    /// Base DbContext that will add entities on the fly based on an array of types
    /// </summary>
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
            foreach (var t in _types)
            {
                if (t.Keyless)
                {
#if NETSTANDARD2_0
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
                    modelBuilder.Entity(t.Type).HasKey(t.PrimaryKeys);
                }
                string tableName = string.IsNullOrEmpty(t.TableName) ? t.Type.Name : t.TableName;
                string schema = string.IsNullOrEmpty(t.Schema) ? null : t.Schema;
                if (t.Keyless)
                {
#if NETSTANDARD2_0
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
