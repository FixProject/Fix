using System;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fix.AspNet
{
    using UseAction = Action<Func<IDictionary<string, object>, Func<Task>, Task>>;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal class AppFuncBuilder
    {
        private const BindingFlags MethodBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static AppFunc Build()
        {
            var fixerSetupMethod = FindFixerSetupMethod();
            var fixer = new Fixer();

            object instance = fixerSetupMethod.IsStatic
                ? null
                : Activator.CreateInstance(fixerSetupMethod.DeclaringType);

            var parameters = new object[1];

            if (fixerSetupMethod.GetParameters().Single().ParameterType.IsAssignableFrom(typeof (Fixer)))
            {
                parameters[0] = fixer;
            }
            else
            {
                parameters[0] = new UseAction(f => fixer.Use(f));
            }

            fixerSetupMethod.Invoke(instance, parameters);
            return fixer.Build();
        }

        private static MethodInfo FindFixerSetupMethod()
        {
            var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray();

            var fixerSetupMethods = assemblies
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
                   type.GetMethods(MethodBindingFlags).FirstOrDefault(MethodTakesUseActionParameter);
        }

        private static bool MethodTakesFixerParameter(MethodInfo methodInfo)
        {
            try
            {
                var parameters = methodInfo.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(Fixer)) && parameters[0].ParameterType != typeof(object);
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
                return parameters.Length == 1 && parameters[0].ParameterType == typeof (UseAction);
            }
            catch
            {
                return false;
            }
        }
    }
}
