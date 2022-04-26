using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Text;
#if NET6_0_OR_GREATER
using System.Runtime.Loader;
#endif

namespace EFPosh.AssemblyLoader
{
#if NET6_0_OR_GREATER
    public class EFPoshAssemblyResolverEventHandler : IModuleAssemblyInitializer, IModuleAssemblyCleanup
    {
        private static EFPoshAssemblyLoadContext _loadContext;
        public EFPoshAssemblyResolverEventHandler()
        {
            _loadContext = new EFPoshAssemblyLoadContext();
        }
        public void OnImport()
        {
            AssemblyLoadContext.Default.Resolving += ResolveAlcEngine;
        }

        public void OnRemove(PSModuleInfo psModuleInfo)
        {
            AssemblyLoadContext.Default.Resolving -= ResolveAlcEngine;
        }

        private static Assembly ResolveAlcEngine(AssemblyLoadContext defaultAlc, AssemblyName assemblyToResolve)
        {
            if (!assemblyToResolve.Name.Contains("EFPosh"))
            {
                return null;
            }
            var path = _loadContext.GetAssemblyDependnecyPath(assemblyToResolve.Name);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return _loadContext.LoadFromAssemblyName(assemblyToResolve);
        }
    }
#endif
}
