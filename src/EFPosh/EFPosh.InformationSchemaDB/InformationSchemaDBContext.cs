using EFPosh.InformationSchemaDB.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace EFPosh.InformationSchemaDB
{
    public class InformationSchemaDBContext : DbContext
    {

        public InformationSchemaDBContext(DbContextOptions options) : base(options)
        {

        }
        public DbQuery<MSSQLInformationSchemaColumns> MSSQLInformationSchemaColumns { get; set; }
    }
}
