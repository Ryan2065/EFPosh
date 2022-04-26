using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#if NET6_0_OR_GREATER
using System.Runtime.Loader;
#endif
using System.Text;

namespace EFPosh.AssemblyLoader
{
#if NET6_0_OR_GREATER
    public class EFPoshAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly List<string> SearchLocations = new List<string>();
        public EFPoshAssemblyLoadContext()
        {
            var assemblyLocation = typeof(EFPoshAssemblyLoadContext).Assembly.Location;
            // assembly is stored in Dependencies\EFPosh\<Framework> - Need to get to Dependencies\EFPosh.EFPoshInteractions\<Framework>
            var assemblyLocationParent = System.IO.Directory.GetParent(assemblyLocation);
            var frameworkName = assemblyLocationParent.Name;



            var interactionsFolder = Path.Combine(assemblyLocationParent.Parent.FullName, "EFPosh.EFPoshInteractions", frameworkName);
            
            SearchLocations.Add(interactionsFolder);
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            foreach(var searchLocation in SearchLocations)
            {
                string assemblyPath = Path.Combine(searchLocation,$"{assemblyName.Name}.dll");
                if (File.Exists(assemblyPath))
                {
                    return LoadFromAssemblyPath(assemblyPath);
                }
            }
            
            return null;
        }
    }
#endif
}
