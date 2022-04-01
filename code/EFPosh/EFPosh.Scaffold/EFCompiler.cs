using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;

#pragma warning disable EF1001 // Internal EF Core API usage.

namespace EFPosh.Scaffold
{
    public class EFCompiler
    {
        private readonly string _directoryToCompile;
        private readonly ScaffoldDbType _dbType;
        private readonly string _dbName;
        public EFCompiler(string directoryToCompile, ScaffoldDbType dbType, string dbName)
        {
            _directoryToCompile = directoryToCompile;
            _dbType = dbType;
            _dbName = dbName;
        }
        private CSharpCompilation GenerateCode(List<string> sourceFiles)
        {
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8);
            var parsedSyntaxTrees = sourceFiles
                .Select(f => SyntaxFactory.ParseSyntaxTree(f, options));
            return CSharpCompilation.Create($"DataContext.dll",
                parsedSyntaxTrees,
                references: GetCompilationReferences(),
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release));
        }
        private List<MetadataReference> GetCompilationReferences()
        {
            var refs = new List<MetadataReference>();
            // Reference all assemblies referenced by this program 
            var referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            refs.AddRange(referencedAssemblies.Select(a =>
                MetadataReference.CreateFromFile(Assembly.Load(a).Location)));
            // Add the missing ones needed to compile the assembly:
            refs.Add(MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(
                Assembly.Load("netstandard, Version=2.1.0.0").Location));
            refs.Add(MetadataReference.CreateFromFile(
                typeof(System.Data.Common.DbConnection).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(
                typeof(System.Linq.Expressions.Expression).Assembly.Location));
            refs.Add(MetadataReference.CreateFromFile(
                typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly.Location));
            switch (_dbType)
            {
                case ScaffoldDbType.SQLite:
                    refs.Add(MetadataReference.CreateFromFile(
                        typeof(Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal.SqliteDatabaseModelFactory).Assembly.Location));
                    break;
                case ScaffoldDbType.SQLServer:
                    refs.Add(MetadataReference.CreateFromFile(
                        typeof(Microsoft.EntityFrameworkCore.SqlServer.Scaffolding.Internal.SqlDataReaderExtension).Assembly.Location));
                    break;
            }
            return refs;
        }
        public bool Compile()
        {
            List<string> sourceFiles = new List<string>();
            var files = System.IO.Directory.GetFiles(_directoryToCompile);
            foreach (var f in files)
            {
                sourceFiles.Add(System.IO.File.ReadAllText(f));
            }
            var generatedCode = GenerateCode(sourceFiles);
            var result = generatedCode.Emit(Path.Combine( _directoryToCompile, _dbName, ".dll" ));
            return result.Success;
        }
    }
}
