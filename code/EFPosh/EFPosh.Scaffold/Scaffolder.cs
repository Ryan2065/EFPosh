using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable EF1001 // Internal EF Core API usage.

namespace EFPosh.Scaffold
{
    public class Scaffolder
    {
        private readonly string _dbName;
        private readonly string[]? _dbTables;
        private readonly string _connectionString;
        private readonly string _outputFolder;
        private readonly ScaffoldDbType _dbType;
        public Scaffolder(string dbName, string connectionString, string outputFolder, ScaffoldDbType dbType, string[]? dbTables = null)
        {
            _dbName = dbName;
            _connectionString = connectionString;
            _outputFolder = outputFolder;
            _dbType = dbType;
            _dbTables = dbTables;
        }
        private IReverseEngineerScaffolder? GetREScaffolder()
        {
            switch (_dbType)
            {
                case ScaffoldDbType.SQLite:
                    return new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .AddLogging()
                        .AddEntityFrameworkDesignTimeServices()
                        .AddSingleton<LoggingDefinitions, SqliteLoggingDefinitions>()
                        .AddSingleton<IRelationalTypeMappingSource, SqliteTypeMappingSource>()
                        .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                        .AddSingleton<IDatabaseModelFactory, SqliteDatabaseModelFactory>()
                        .AddSingleton<IProviderConfigurationCodeGenerator, SqliteCodeGenerator>()
                        .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                        .AddSingleton<ProviderCodeGeneratorDependencies>()
                        .AddSingleton<AnnotationCodeGeneratorDependencies>()
                        .BuildServiceProvider()
                        .GetRequiredService<IReverseEngineerScaffolder>();
                case ScaffoldDbType.SQLServer:
                    return new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .AddLogging()
                        .AddEntityFrameworkDesignTimeServices()
                        .AddSingleton<LoggingDefinitions, SqlServerLoggingDefinitions>()
                        .AddSingleton<IRelationalTypeMappingSource, SqlServerTypeMappingSource>()
                        .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                        .AddSingleton<IDatabaseModelFactory, SqlServerDatabaseModelFactory>()
                        .AddSingleton<IProviderConfigurationCodeGenerator, SqlServerCodeGenerator>()
                        .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                        .AddSingleton<ProviderCodeGeneratorDependencies>()
                        .AddSingleton<AnnotationCodeGeneratorDependencies>()
                        .BuildServiceProvider()
                        .GetRequiredService<IReverseEngineerScaffolder>();
                default:
                    return null;
            }
        }
        private DatabaseModelFactoryOptions GetFactoryOptions()
        {
            if(_dbTables != null)
            {
                return new DatabaseModelFactoryOptions(_dbTables);
            }
            return new DatabaseModelFactoryOptions();
        }
        public void Start()
        {
            var scaffolder = GetREScaffolder();
            if(scaffolder == null)
            {
                throw new NullReferenceException($"Could not generate scaffolder for {_dbName}");
            }
            var modelRevEngOpts = new ModelReverseEngineerOptions();
            modelRevEngOpts.NoPluralize = true;
            modelRevEngOpts.UseDatabaseNames = true;

            var codeGeneratorOps = new ModelCodeGenerationOptions()
            {
                RootNamespace = $"EFPosh{_dbName}",
                ContextName = $"{_dbName}Context",
                ContextNamespace = $"EFPosh{_dbName}.Context",
                ModelNamespace = $"EFPosh{_dbName}.Models",
                SuppressConnectionStringWarning = true,
                UseDataAnnotations = true,
                SuppressOnConfiguring = true
            };

            var scaffoldedModel = scaffolder.ScaffoldModel(_connectionString, GetFactoryOptions(), modelRevEngOpts, codeGeneratorOps);
            scaffolder.Save(scaffoldedModel, _outputFolder, true);

        }
    }
    public enum ScaffoldDbType
    {
        SQLite,
        SQLServer
    }
}