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
        private readonly ExportCache _cache;

        public FunctionExportProvider(Assembly assembly, params Type[] delegateTypes) : this(assembly, delegateTypes.AsEnumerable())
        {
            
        }

        public FunctionExportProvider(Assembly assembly, IEnumerable<Type> delegateTypes)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (delegateTypes == null) throw new ArgumentNullException("delegateTypes");
            if (!delegateTypes.Any()) throw new ArgumentException("Must specify at least one delegate type.");
            if (delegateTypes.Any(t => !typeof(Delegate).IsAssignableFrom(t))) throw new NotSupportedException("Only delegate types are supported.");

            _cache = new ExportCache(assembly, delegateTypes);
        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            return _cache.GetExports().Where(export => definition.IsConstraintSatisfiedBy(export.Definition));
        }
    }
}
