using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FixUp
{
    class DelegateFactory
    {
        private readonly ConcurrentDictionary<Type, object> _instances = new ConcurrentDictionary<Type, object>();

        public Delegate CreateDelegate(Type delegateType, MethodInfo methodInfo)
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
