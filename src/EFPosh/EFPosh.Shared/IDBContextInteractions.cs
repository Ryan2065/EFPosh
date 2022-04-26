using System;
using System.Collections.Generic;
using System.Text;

namespace EFPosh.Shared
{
    public interface IDBContextInteractions
    {
        void NewPoshContext(
            string connectionString, 
            string dbType, 
            PoshEntity[] Types, 
            bool EnsureCreated, 
            bool RunMigrations, 
            bool ReadOnly
        );
        void ExistingContext(
            string connectionString,
            string dbType,
            bool EnsureCreated,
            bool RunMigrations,
            bool ReadOnly,
            string dllPath,
            string ContextClassName
        );
    }
}
