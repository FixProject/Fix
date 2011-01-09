using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace FixUp
{
    class ExportCache
    {
        private readonly DelegateFactory _delegateFactory = new DelegateFactory();
        private readonly IEnumerable<Type> _delegateTypes;
        private IEnumerable<Export> _internalCache;
        private readonly Assembly _assembly;

        public ExportCache(Assembly assembly, IEnumerable<Type> delegateTypes)
        {
            _assembly = assembly;
            _delegateTypes = delegateTypes;
        }

        public IEnumerable<Export> GetExports()
        {
            return _internalCache ?? EnumerateAndCacheExports();
        }

        private IEnumerable<Export> EnumerateAndCacheExports()
        {
            var cache = new List<Export>();
            foreach (var export in EnumerateExports())
            {
                cache.Add(export);
                yield return export;
            }
            Interlocked.CompareExchange(ref _internalCache, cache, null);
        }

        private IEnumerable<Export> EnumerateExports()
        {
            var methodFinder = new MethodFinder(_assembly);
            return from delegateType in _delegateTypes
                   from method in methodFinder.GetAllMethods(delegateType)
                   let contractName = method.ToContractName()
                   let exportDefinition = new ExportDefinition(contractName, new Dictionary<string, object> { { "ExportTypeIdentity", contractName } })
                   select CreateExport(exportDefinition, delegateType, method);
        }


        private Export CreateExport(ExportDefinition exportDefinition, Type delegateType, MethodInfo methodInfo)
        {
            return new Export(exportDefinition, () => _delegateFactory.CreateDelegate(delegateType, methodInfo));
        }
    }
}
