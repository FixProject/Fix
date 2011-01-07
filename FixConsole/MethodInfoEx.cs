using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FixUp
{
    static class MethodInfoEx
    {
        private static readonly TypeAssignableComparer Comparer = new TypeAssignableComparer();

        public static bool HasSameSignatureAs(this MethodInfo first, MethodInfo second)
        {
            if (first.ReturnType != second.ReturnType) return false;
            if (first.GetParameters().Length != second.GetParameters().Length) return false;
            var result = first.GetParameters().Select(p => p.ParameterType)
                .SequenceEqual(second.GetParameters().Select(p => p.ParameterType), Comparer);
            return result;
        }

        public static string ToContractName(this MethodInfo methodInfo)
        {
            var genericParameterList = string.Join(",", methodInfo.GetParameters().Select(pi => GenericTypeNameWithoutAssembly(pi.ParameterType)));
            return methodInfo.ReturnType.FullName + "(" + genericParameterList + ")";
        }

        private static string GenericTypeNameWithoutAssembly(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (!type.IsGenericType) return type.FullName;
            return string.Format("{0}({1})", type.FullName.Split('`')[0],
                                 string.Join(",", type.GetGenericArguments().Select(GenericTypeNameWithoutAssembly)));
        }

        private class TypeAssignableComparer : IEqualityComparer<Type>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
            public bool Equals(Type x, Type y)
            {
                return x.IsAssignableFrom(y);
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <returns>
            /// A hash code for the specified object.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
            public int GetHashCode(Type obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
