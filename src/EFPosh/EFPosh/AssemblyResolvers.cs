using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Dynamic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using PoshLogger;
#if NET6_0
using Microsoft.Extensions.DependencyModel;
using System.Runtime.Loader;
using System.Text.Json.Nodes;
#endif

namespace EFPosh
{
    public static class AssemblyResolvers
    {
        private static readonly PoshILogger _logger = new(LogLevel.Information);
        private static void FindFoldersToCheckForDlls()
        {
            var assemblyFolder = Path.GetDirectoryName(typeof(PoshContextInteractions).Assembly.Location);
            if (string.IsNullOrEmpty(assemblyFolder)) { return; }
            if (!DllPathsToCheck.Contains(assemblyFolder))
            {
                DllPathsToCheck.Add(assemblyFolder);
            }
        }

        public static bool LoadedSqlClientResolver { get; set; } = false;
        public static bool LoadedSqliteResolver { get; set; } = false;
        public static List<string> DllPathsToCheck { get; set; } = new();

#if NET6_0
        public static void LoadSqlClient()
        {
            if (LoadedSqlClientResolver) { return; }
            FindFoldersToCheckForDlls();

            foreach (var dllPath in DllPathsToCheck)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var pathToDll = Path.Combine(dllPath, "runtimes", "win", "lib", "netcoreapp3.1", "Microsoft.Data.SqlClient.dll");
                    if (File.Exists(pathToDll))
                    {
                        _logger.LogTrace("Manually loading {pathToDll}", pathToDll);
                        System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(pathToDll);
                        var assemb = Assembly.Load("Microsoft.Data.SqlClient");
                        NativeLibrary.SetDllImportResolver(Assembly.Load("Microsoft.Data.SqlClient"), AssemblyResolvers.NativeAssemblyResolverSqlClient);
                    }
                    LoadedSqlClientResolver = true;
                    return;
                }
                else
                {
                    var pathToDll = Path.Combine(dllPath, "runtimes", "unix", "lib", "netcoreapp3.1", "Microsoft.Data.SqlClient.dll");
                    if (File.Exists(pathToDll))
                    {
                        _logger.LogTrace("Manually loading {pathToDll}", pathToDll);
                        System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(pathToDll);
                    }
                    LoadedSqlClientResolver = true;
                    return;
                }
            }
        }
        internal static IntPtr NativeAssemblyResolverSqlClient(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (!libraryName.Equals("Microsoft.Data.SqlClient.SNI.dll", StringComparison.OrdinalIgnoreCase) || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return IntPtr.Zero;
            }
            FindFoldersToCheckForDlls();
            IntPtr libHandle = IntPtr.Zero;
            string runtimeId = "win-x64";
            if(RuntimeInformation.OSArchitecture == Architecture.X86)
            {
                runtimeId = "win-x86";
            }
            foreach (var dllPath in DllPathsToCheck)
            {
                var pathToDll = Path.Combine(dllPath, "runtimes", runtimeId, "native", "Microsoft.Data.SqlClient.SNI.dll");
                if (File.Exists(pathToDll))
                {
                    _logger.LogTrace("Manually loading {pathToDll}", pathToDll);
                    libHandle = NativeLibrary.Load(pathToDll, assembly, searchPath);
                    return libHandle;
                   
                }
            }
            return libHandle;
        }
        internal static IntPtr NativeAssemblyResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (!libraryName.Equals("e_sqlite3", StringComparison.OrdinalIgnoreCase))
            {
                return IntPtr.Zero;
            }
            FindFoldersToCheckForDlls();
            IntPtr libHandle = IntPtr.Zero;
            foreach(var currentFolder in DllPathsToCheck)
            {
                string dllPath = "";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.X64)
                {
                    dllPath = Path.Combine(currentFolder, "runtimes", "win-x64", "native", "e_sqlite3.dll");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.OSArchitecture == Architecture.X86)
                {
                    dllPath = Path.Combine(currentFolder, "runtimes", "win-x86", "native", "e_sqlite3.dll");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture == Architecture.X64)
                {
                    dllPath = Path.Combine(currentFolder, "runtimes", "linux-x64", "native", "libe_sqlite3.so");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    dllPath = Path.Combine(currentFolder, "runtimes", "osx", "native", "libe_sqlite3.dylib");
                }
                if (File.Exists(dllPath))
                {
                    _logger.LogTrace("Manually loading {dllPath}", dllPath);
                    NativeLibrary.TryLoad(dllPath, assembly, searchPath, out libHandle);
                    return libHandle;
                }
            }
            
            return libHandle;
        }
#endif
        internal static Assembly PoshResolveEventHandler(object sender, ResolveEventArgs args)
        {
            FindFoldersToCheckForDlls();
            var dllNeeded = args.Name.Split(',')[0] + ".dll";
            foreach (var directoryInfo in DllPathsToCheck)
            {
                var fullDLLPath = Path.Combine(directoryInfo, dllNeeded);
                if (File.Exists(fullDLLPath))
                {
                    _logger.LogTrace("Manually loading {fullDLLPath}", fullDLLPath);
                    return Assembly.LoadFrom(fullDLLPath);
                }
            }
            return null;
        }
    }
}
