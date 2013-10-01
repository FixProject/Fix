using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fix
{
    using System.Reflection;

    public static class AssemblyManager
    {
        private static List<Assembly> _cache;

        public static IEnumerable<Assembly> GetReferencedAssemblies()
        {
            return _cache ?? FindReferencedAssemblies();
        }

        private static IEnumerable<Assembly> FindReferencedAssemblies()
        {
            var assembly = Assembly.GetEntryAssembly();
            var cache = new List<Assembly> {assembly};
            yield return assembly;

            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == referencedAssembly.FullName) ??
                           Assembly.Load(referencedAssembly);
                cache.Add(assembly);
                yield return assembly;
            }

            _cache = cache;
        }
    }
}
