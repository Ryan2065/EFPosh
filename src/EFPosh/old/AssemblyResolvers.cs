/*using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Dynamic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
#if NET6_0
using Microsoft.Extensions.DependencyModel;
using System.Runtime.Loader;
using System.Text.Json.Nodes;
#endif

namespace EFPosh
{
    /*public static class AssemblyResolvers
    {
        public static List<string> dllPathsToCheck = new List<string>();
        private static void FindFoldersToCheckForDlls()
        {
            
            var assemblyFolder = Path.GetDirectoryName(typeof(PoshContextInteractions).Assembly.Location);
            if (string.IsNullOrEmpty(assemblyFolder)) { return; }
            if (!dllPathsToCheck.Contains(assemblyFolder))
            {
                dllPathsToCheck.Add(assemblyFolder);
            }
        }
#if NET6_0
        internal static IntPtr NativeAssemblyResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            Console.WriteLine(libraryName);
            if (!libraryName.Equals("e_sqlite3", StringComparison.OrdinalIgnoreCase))
                return IntPtr.Zero;
            FindFoldersToCheckForDlls();
            IntPtr libHandle = IntPtr.Zero;
            foreach(var currentFolder in dllPathsToCheck)
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
            Console.WriteLine($"{args.Name}");
            var dllNeeded = args.Name.Split(',')[0] + ".dll";
            foreach (var directoryInfo in dllPathsToCheck)
            {
                var fullDLLPath = Path.Combine(directoryInfo, dllNeeded);
                if (File.Exists(fullDLLPath))
                {
                    return Assembly.LoadFrom(fullDLLPath);
                }
            }
            return null;
        }
    }
}
*/