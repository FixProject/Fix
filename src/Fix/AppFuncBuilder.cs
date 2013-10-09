using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fix
{
    using UseAction = Action<Func<IDictionary<string, object>, Func<Task>, Task>>;

    public class AppFuncBuilder
    {
        private static IList<IFixerAdapter> _adapters;
        private readonly IList<Assembly> _assemblies;

        private AppFuncBuilder(IList<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }

        public static AppFuncBuilder Create()
        {
            var assemblies = AssemblyManager.GetReferencedAssemblies().ToList();
            if (_adapters == null)
            {
                LoadAdapters(assemblies);
            }
            return new AppFuncBuilder(assemblies);
        }
        
        public static AppFuncBuilder Create(IEnumerable<Assembly> assemblies)
        {
            var list = assemblies.ToList();
            if (_adapters == null)
            {
                LoadAdapters(list);
            }
            return new AppFuncBuilder(list);
        }

        private static void LoadAdapters(IEnumerable<Assembly> assemblies)
        {
            var adapters =
                assemblies.SelectMany(a => a.GetExportedTypes().Where(t => !t.IsInterface).Where(typeof (IFixerAdapter).IsAssignableFrom))
                    .Distinct()
                    .Select(Activator.CreateInstance)
                    .Cast<IFixerAdapter>();
            _adapters = new List<IFixerAdapter>(adapters);
        }

        private const BindingFlags MethodBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public Func<IDictionary<string, object>, Task> Build()
        {
            var fixerSetupMethod = FindFixerSetupMethod();
            var fixer = new Fixer();

            object instance = fixerSetupMethod.IsStatic || fixerSetupMethod.DeclaringType == null
                ? null
                : Activator.CreateInstance(fixerSetupMethod.DeclaringType);

            var parameters = new object[1];

            var parameterType = fixerSetupMethod.GetParameters().Single().ParameterType;
            if (parameterType.IsAssignableFrom(typeof (Fixer)))
            {
                parameters[0] = fixer;
            }
            else
            {
                var adapter = _adapters.FirstOrDefault(a => parameterType.IsAssignableFrom(a.AdaptedType));
                if (adapter != null)
                {
                    parameters[0] = adapter.Adapt(fixer);
                }
            }
            if (parameters[0] == null)
            {
                parameters[0] = new Action<Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>>(f => fixer.Use(f));
            }

            fixerSetupMethod.Invoke(instance, parameters);
            return fixer.Build();
        }

        private MethodInfo FindFixerSetupMethod()
        {
            var fixerSetupMethods = _assemblies
                .SelectMany(a => a.GetExportedTypes().Where(t => t.Name.Equals("OwinAppSetup")).Select(FixerMethod))
                .Where(m => !ReferenceEquals(m, null))
                .ToList();

            if (fixerSetupMethods.Count == 0)
                throw new EntryPointNotFoundException(
                    "No type was found called OwinAppSetup with a method taking a valid parameter type.");
            if (fixerSetupMethods.Count > 1)
                throw new EntryPointNotFoundException(
                    "More than one type was found called OwinAppSetup with a method taking a valid parameter type.");

            var fixerSetupMethod = fixerSetupMethods.Single();
            return fixerSetupMethod;
        }

        private static MethodInfo FixerMethod(Type type)
        {
            return type.GetMethods(MethodBindingFlags).FirstOrDefault(MethodTakesFixerParameter)
                   ??
                   type.GetMethods(MethodBindingFlags).FirstOrDefault(MethodTakesUseActionParameter)
                   ??
                   type.GetMethods(MethodBindingFlags).FirstOrDefault(MethodTakesAdapterParameter);
        }

        private static bool MethodTakesAdapterParameter(MethodInfo methodInfo)
        {
            try
            {
                var parameters = methodInfo.GetParameters();
                return parameters.Length == 1 && _adapters.Any(a => parameters[0].ParameterType == a.AdaptedType);
            }
            catch
            {
                return false;
            }
        }

        private static bool MethodTakesFixerParameter(MethodInfo methodInfo)
        {
            try
            {
                var parameters = methodInfo.GetParameters();
                return parameters.Length == 1
                       &&
                       parameters[0].ParameterType == typeof(Fixer);
            }
            catch
            {
                return false;
            }
        }
        
        private static bool MethodTakesUseActionParameter(MethodInfo methodInfo)
        {
            try
            {
                var parameters = methodInfo.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(Action<Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>>);
            }
            catch
            {
                return false;
            }
        }
    }
}
