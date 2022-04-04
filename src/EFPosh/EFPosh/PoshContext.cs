using Microsoft.EntityFrameworkCore;

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
                    modelBuilder.Entity(t.Type).HasKey(t.PrimaryKeys);
                }
                string tableName = string.IsNullOrEmpty(t.TableName) ? t.TableName : t.Type.Name;
                string schema = string.IsNullOrEmpty(t.Schema) ? t.Schema : null;
                if (t.Keyless)
                {
#if NETFRAMEWORK
                    modelBuilder.Query(t.Type).ToView(t.TableName, schema);
#else
                    modelBuilder.Entity(t.Type).ToView(t.TableName, schema);
#endif
                }
                else
                {
                    modelBuilder.Entity(t.Type).ToTable(t.TableName, schema);
                }
            }
        }
    }
}
