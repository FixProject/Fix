using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fix
{
    using UseAction = Action<Func<IDictionary<string, object>, Func<Task>, Task>>;
    using AppFunc = Func<IDictionary<string,object>, Task>;

    public class AppFuncBuilder
    {
        private readonly IDictionary<string, object> _startupEnv;
        private static IList<IFixerAdapter> _adapters;
        private readonly IList<Assembly> _assemblies;

        private AppFuncBuilder(IList<Assembly> assemblies, IDictionary<string, object> startupEnv)
        {
            _assemblies = assemblies;
            _startupEnv = startupEnv;
        }

        public static AppFuncBuilder Create()
        {
            return Create(new Dictionary<string, object>());
        }

        public static AppFuncBuilder Create(IDictionary<string, object> startupEnv)
        {
            var assemblies = AssemblyManager.GetReferencedAssemblies().ToList();
            if (_adapters == null)
            {
                LoadAdapters(assemblies);
            }
            return new AppFuncBuilder(assemblies, startupEnv);
        }

        public static AppFuncBuilder Create(IEnumerable<Assembly> assemblies)
        {
            return Create(assemblies, new Dictionary<string, object>());
        }

        public static AppFuncBuilder Create(IEnumerable<Assembly> assemblies, IDictionary<string, object> startupEnv)
        {
            var list = assemblies.ToList();
            if (_adapters == null)
            {
                LoadAdapters(list);
            }
            return new AppFuncBuilder(list, startupEnv);
        }

        private static void LoadAdapters(IEnumerable<Assembly> assemblies)
        {
            var adapters =
                assemblies.SelectMany(TryGetExportedFixerAdapterTypes)
                    .Distinct()
                    .Select(Activator.CreateInstance)
                    .Cast<IFixerAdapter>();
            _adapters = new List<IFixerAdapter>(adapters);
        }

        private static IEnumerable<Type> TryGetExportedFixerAdapterTypes(Assembly a)
        {
            try
            {
                return a.GetExportedTypes().Where(t => !t.IsInterface).Where(typeof(IFixerAdapter).IsAssignableFrom);
            }
            catch (Exception)
            {
                return Enumerable.Empty<Type>();
            }
        }

        private const BindingFlags MethodBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public Func<IDictionary<string, object>, Task> Build()
        {
            var fixerSetupMethod = FindFixerSetupMethod();
            var fixer = new Fixer();

            object instance = fixerSetupMethod.IsStatic || fixerSetupMethod.DeclaringType == null
                ? null
                : CreateInstanceOfOwinAppSetupClass(fixerSetupMethod.DeclaringType);

            object[] parameters;

            var parameterInfos = fixerSetupMethod.GetParameters();
            if (parameterInfos.Length == 1)
            {
                parameters = new object[1];
                var parameterType = parameterInfos[0].ParameterType;
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
                    parameters[0] = new Action<Func<AppFunc, AppFunc>>(f => fixer.Use(f));
                }
            }
            else
            {
                var useAction = new Action<Func<AppFunc, AppFunc>>(f => fixer.Use(f));
                var mapAction = new Action<string, Func<AppFunc, AppFunc>>((p,f) => fixer.Map(p,f));
                parameters = new object[]{useAction, mapAction};
            }

            fixerSetupMethod.Invoke(instance, parameters);
            return fixer.Build();
        }

        private object CreateInstanceOfOwinAppSetupClass(Type type)
        {
            var constructors = type.GetConstructors();
            var withStartupEnv = (from constructor in constructors
                                      let parameters = constructor.GetParameters()
                                      where parameters.Length == 1 && parameters[0].ParameterType == typeof(IDictionary<string, object>)
                                      select constructor).FirstOrDefault();
            if (withStartupEnv != null)
            {
                return Activator.CreateInstance(type, _startupEnv);
            }
            return Activator.CreateInstance(type);
        }

        private MethodInfo FindFixerSetupMethod()
        {
            var fixerSetupMethods = _assemblies
                .SelectMany(TryGetExportedFixerMethods)
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

        private static IEnumerable<MethodInfo> TryGetExportedFixerMethods(Assembly a)
        {
            try
            {
                return a.GetExportedTypes().Where(t => t.Name.Equals("OwinAppSetup")).Select(FixerMethod);
            }
            catch (Exception)
            {
                return Enumerable.Empty<MethodInfo>();
            }
        }

        private static MethodInfo FixerMethod(Type type)
        {
            return type.GetMethods(MethodBindingFlags).FirstOrDefault(MethodTakesFixerParameter)
                   ??
                   type.GetMethods(MethodBindingFlags).FirstOrDefault(MethodTakesUseAndMapActionParameters)
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
            var actionType = typeof (Action<Func<AppFunc, AppFunc>>);
            try
            {
                var parameters = methodInfo.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == actionType;
            }
            catch
            {
                return false;
            }
        }
        private static bool MethodTakesUseAndMapActionParameters(MethodInfo methodInfo)
        {
            var useType = typeof (Action<Func<AppFunc, AppFunc>>);
            var mapType = typeof (Action<string, Func<AppFunc, AppFunc>>);
            try
            {
                var parameters = methodInfo.GetParameters();
                return parameters.Length == 2
                    && parameters[0].ParameterType == useType
                    && parameters[1].ParameterType == mapType;
            }
            catch
            {
                return false;
            }
        }
    }
}
