using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FixUp
{
    class MethodFinder
    {
        private readonly Assembly _assembly;

        public MethodFinder(Assembly assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<MethodInfo> GetAllMethods(Type delegateType)
        {
            var invokeMethodInfo = delegateType.GetMethod("Invoke");
            if (invokeMethodInfo == null) throw new ArgumentException("Type must be a delegate type.");
            return GetAllMethods()
                .Where(methodInfo => invokeMethodInfo.HasSameSignatureAs(methodInfo));
        }

        public IEnumerable<MethodInfo> GetAllMethods()
        {
            return GetInstanceMethods().Concat(GetStaticMethods());
        }

        private IEnumerable<MethodInfo> GetInstanceMethods()
        {
            return GetMatchingMethods(BindingFlags.Public | BindingFlags.Instance);
        }

        private IEnumerable<MethodInfo> GetStaticMethods()
        {
            return GetMatchingMethods(BindingFlags.Public | BindingFlags.Static);
        }

        private IEnumerable<MethodInfo> GetMatchingMethods(BindingFlags bindingFlags)
        {
            return from exportedType in _assembly.GetExportedTypes()
                   from methodInfo in exportedType.GetMethods(bindingFlags)
                   select methodInfo;
        }
    }
}
