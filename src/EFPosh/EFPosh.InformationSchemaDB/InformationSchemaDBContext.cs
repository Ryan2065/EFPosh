using EFPosh.InformationSchemaDB.Models;
using Microsoft.EntityFrameworkCore;

namespace EFPosh.InformationSchemaDB
{
    public class InformationSchemaDBContext : DbContext
    {

        public InformationSchemaDBContext(DbContextOptions options) : base(options)
        {

        }
#if NETFRAMEWORK
        public DbQuery<MSSQLInformationSchemaColumns> MSSQLInformationSchemaColumns { get; set; }
#else
        public DbSet<MSSQLInformationSchemaColumns> MSSQLInformationSchemaColumns { get; set; }
#endif
    }
}
