using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace EFPosh.ALC
{
    public class LoadContext : AssemblyLoadContext
    {
        private readonly string _dependencyDirectory;
        public LoadContext(string dependencyDirectory)
        {
            _dependencyDirectory = dependencyDirectory;
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = Path.Combine(_dependencyDirectory,$"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
    public static class LoadContextResolver 
    {
        private static LoadContext PoshLoadContext = null;
        private static string _dependencyDir = "";
        public static void LoadResolver(string dependencyDir)
        {
            _dependencyDir = dependencyDir;
            PoshLoadContext = new LoadContext(dependencyDir);
            PoshLoadContext.LoadFromAssemblyPath(System.IO.Path.Combine(dependencyDir, "EFPosh.dll"));
            PoshLoadContext.LoadFromAssemblyPath(System.IO.Path.Combine(dependencyDir, "Microsoft.Data.Sqlite.dll"));
            /*string winPath = Path.Combine(_dependencyDir, "runtimes", "win", "lib", "net6.0", $"{assemblyToResolve.Name}.dll");
            string linuxPath = Path.Combine(_dependencyDir, "runtimes", "linux", "lib", "net6.0", $"{assemblyToResolve.Name}.dll");
            string macPath = Path.Combine(_dependencyDir, "runtimes", "osx", "lib", "net6.0", $"{assemblyToResolve.Name}.dll");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(winPath))
            {
                return PoshLoadContext.LoadFromAssemblyPath(winPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && File.Exists(linuxPath))
            {
                return PoshLoadContext.LoadFromAssemblyPath(linuxPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && File.Exists(macPath))
            {
                return PoshLoadContext.LoadFromAssemblyPath(macPath);
            }*/
            AssemblyLoadContext.Default.Resolving += ResolveAlcEngine;
        }

        public static void RemoveResolver()
        {
            AssemblyLoadContext.Default.Resolving -= ResolveAlcEngine;
        }

        private static Assembly ResolveAlcEngine(AssemblyLoadContext defaultAlc, AssemblyName assemblyToResolve)
        {
            string filePath = Path.Combine(_dependencyDir, $"{assemblyToResolve.Name}.dll");
            string winPath = Path.Combine(_dependencyDir, "runtimes", "win", "lib", "net6.0", $"{assemblyToResolve.Name}.dll");
            string linuxPath = Path.Combine(_dependencyDir, "runtimes", "linux", "lib", "net6.0", $"{assemblyToResolve.Name}.dll");
            string macPath = Path.Combine(_dependencyDir, "runtimes", "osx", "lib", "net6.0", $"{assemblyToResolve.Name}.dll");
            Console.WriteLine($"Looking for {filePath}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(winPath))
            {
                Console.WriteLine($"Loading correct version in {winPath}");
                return PoshLoadContext.LoadFromAssemblyPath(winPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && File.Exists(linuxPath))
            {
                return PoshLoadContext.LoadFromAssemblyPath(linuxPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && File.Exists(macPath))
            {
                return PoshLoadContext.LoadFromAssemblyPath(macPath);
            }
            else if (File.Exists(filePath))
            {
                return PoshLoadContext.LoadFromAssemblyPath(filePath);
            }
            return null;
        }
    }
}