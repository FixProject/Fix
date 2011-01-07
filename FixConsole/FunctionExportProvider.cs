using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FixUp
{
    public class FunctionExportProvider : ExportProvider
    {
        private readonly Assembly _assembly;
        private readonly IEnumerable<Type> _delegateTypes;
        private readonly ConcurrentDictionary<Type, object> _instances = new ConcurrentDictionary<Type, object>();
        private IEnumerable<Export> _cache;

        public FunctionExportProvider(Assembly assembly, params Type[] delegateTypes) : this(assembly, delegateTypes.AsEnumerable())
        {
            
        }

        public FunctionExportProvider(Assembly assembly, IEnumerable<Type> delegateTypes)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (delegateTypes == null) throw new ArgumentNullException("delegateTypes");
            if (!delegateTypes.Any()) throw new ArgumentException("Must specify at least one delegate type.");
            if (delegateTypes.Any(t => !typeof(Delegate).IsAssignableFrom(t))) throw new NotSupportedException("Only delegate types are supported.");

            _assembly = assembly;
            _delegateTypes = delegateTypes;
        }
        
        private IEnumerable<Export> Cache
        {
            get { return _cache ?? EnumerateAndCacheExports(); }
        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            return Cache.Where(export => definition.IsConstraintSatisfiedBy(export.Definition));
        }

        // Build up aggregateexportprovider composing from FunctionExportProvider

        private IEnumerable<Export> EnumerateAndCacheExports()
        {
            var cache = new List<Export>();
            foreach (var export in EnumerateExports())
            {
                cache.Add(export);
                yield return export;
            }
            Interlocked.CompareExchange(ref _cache, cache, null);
        }

        private IEnumerable<Export> EnumerateExports()
        {
            return from delegateType in _delegateTypes
                   from method in GetAllMethods(delegateType)
                   let contractName = method.ToContractName()
                   let exportDefinition = new ExportDefinition(contractName, new Dictionary<string, object> { { "ExportTypeIdentity", contractName}})
                   select CreateExport(exportDefinition, delegateType, method);
        }

        private IEnumerable<MethodInfo> GetAllMethods(Type delegateType)
        {
            var invokeMethodInfo = delegateType.GetMethod("Invoke");
            if (invokeMethodInfo == null) throw new ArgumentException("Type must be a delegate type.");
            return GetInstanceMethods().Concat(GetStaticMethods())
                .Where(methodInfo => invokeMethodInfo.HasSameSignatureAs(methodInfo));
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
        
        private Export CreateExport(ExportDefinition exportDefinition, Type delegateType, MethodInfo methodInfo)
        {
            return new Export(exportDefinition, () => CreateDelegate(delegateType, methodInfo));
        }

        private Delegate CreateDelegate(Type delegateType, MethodInfo methodInfo)
        {
            if (methodInfo.IsStatic)
            {
                return Delegate.CreateDelegate(delegateType, methodInfo);
            }
            var instance = _instances.GetOrAdd(methodInfo.DeclaringType, Activator.CreateInstance);
            return Delegate.CreateDelegate(delegateType, instance, methodInfo);
        }
    }
}
