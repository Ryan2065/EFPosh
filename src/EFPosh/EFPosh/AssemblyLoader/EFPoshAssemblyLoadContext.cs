using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER
using System.Runtime.Loader;
#endif
using System.Text;

namespace EFPosh.AssemblyLoader
{
#if NET6_0_OR_GREATER
    public class EFPoshAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly Dictionary<string, string> _dependencies = new Dictionary<string, string>();
        private string EntryAssemblyFolder = "";
        public EFPoshAssemblyLoadContext()
        {
            EntryAssemblyFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var assemblyLocation = typeof(EFPoshAssemblyLoadContext).Assembly.Location;
            // assembly is stored in Dependencies\EFPosh\<Framework> - Need to get to Dependencies\EFPosh.EFPoshInteractions\<Framework>
            var assemblyLocationParent = System.IO.Directory.GetParent(assemblyLocation);
            var frameworkName = assemblyLocationParent.Name;

            var interactionsFolder = Path.Combine(assemblyLocationParent.Parent.Parent.FullName, "EFPosh.EFInteractions", frameworkName);

            var envDepFolder = Environment.GetEnvironmentVariable("EFPoshDependencyFolder");
            string dependencyRoot = interactionsFolder;
            if (!string.IsNullOrEmpty(envDepFolder))
            {
                dependencyRoot = envDepFolder;
            }
            
            var rid = RuntimeInformation.RuntimeIdentifier;

            // would like to add the different types of RID that could work for this device.
            // so if the RID is win10-x64, then the RIDs we want to cover are: win10-x64, win10, win-x64, win
            AddIfFolderExists(dependencyRoot, "runtimes", rid);
            var ridNoArch = rid.Split("-")[0];
            AddIfFolderExists(dependencyRoot, "runtimes", ridNoArch);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                AddIfFolderExists(dependencyRoot, "runtimes", $"win-{RuntimeInformation.OSArchitecture}");
                AddIfFolderExists(dependencyRoot, "runtimes", "win");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                AddIfFolderExists(dependencyRoot, "runtimes", $"linux-{RuntimeInformation.OSArchitecture}");
                AddIfFolderExists(dependencyRoot, "runtimes", "linux");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                AddIfFolderExists(dependencyRoot, "runtimes", $"osx-{RuntimeInformation.OSArchitecture}");
                AddIfFolderExists(dependencyRoot, "runtimes", "osx");
            }
            AddIfFolderExists(dependencyRoot);
        }
        private void AddIfFolderExists(string dependencyRoot, string firstRoot = "", string secondRoot = "")
        {
            var path = dependencyRoot;
            if (!string.IsNullOrEmpty(firstRoot))
            {
                path = Path.Combine(path, firstRoot);
            }
            if (!string.IsNullOrEmpty(secondRoot))
            {
                path = Path.Combine(path, secondRoot);
            }
            if (Directory.Exists(path)) {
                try
                {
                    foreach (var dllFile in Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories))
                    {
                        string fileName = Path.GetFileName(dllFile);
                        if (!_dependencies.ContainsKey(fileName))
                        {
                            _dependencies.Add(fileName, dllFile);
                        }
                    }
                }
                catch
                {
                    // above could fail if no permissions or something - so just won't get the dlls in that path
                }
            }
            else
            {
                //Console.WriteLine($"Does not exist: {path}");
            }
        }
        public string GetAssemblyDependnecyPath(string assemblyName)
        {
            var fileName = $"{assemblyName}.dll";
            if (_dependencies.ContainsKey(fileName))
            {
                return _dependencies[fileName];
            }
            return null;
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            
            if(IsSatisfyingAssembly(assemblyName, Path.Combine(EntryAssemblyFolder, $"{assemblyName.Name}.dll")))
            {
                return null;
            }
 
            var path = GetAssemblyDependnecyPath(assemblyName.Name);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (IsSatisfyingAssembly(assemblyName, path))
            {
                return LoadFromAssemblyPath(path);
            }
            return null;
        }
        private static bool IsSatisfyingAssembly(AssemblyName requiredAssemblyName, string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                return false;
            }

            AssemblyName asmToLoadName = AssemblyName.GetAssemblyName(assemblyPath);

            return string.Equals(asmToLoadName.Name, requiredAssemblyName.Name, StringComparison.OrdinalIgnoreCase)
                && asmToLoadName.Version >= requiredAssemblyName.Version;
        }
    }
#endif
}
