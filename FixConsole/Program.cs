using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using Fix;
using Info;
using Print;
using TestModule;
using TestServer;
using Infix = System.Action<System.Collections.Generic.IDictionary<string, string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>, System.Delegate>;
using RequestHandler = System.Action<System.Collections.Generic.IDictionary<string, string>, System.Func<byte[]>, System.Action<int, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>>, System.Func<byte[]>>, System.Action<System.Exception>>;

namespace FixUp
{
    class Program
    {
        static void Main(string[] args)
        {
            var prefix = args.Length == 1 ? args[0] : "http://*:81/";
            using (var server = new Server(prefix))
            {
                var fixer = new Fixer(server.Start, server.Stop);

                var container =
                    new CompositionContainer(CreateAggregateExportProvider(typeof (Infix), typeof (RequestHandler)));
                container.ComposeParts(fixer);

                fixer.Start();
                Console.Write("Running. Press Enter to stop.");
                Console.ReadLine();
                fixer.Stop();
            }
        }

        private static AggregateExportProvider CreateAggregateExportProvider(params Type[] delegateTypes)
        {
            return new AggregateExportProvider(CreateExportProviders(LoadAssembliesInFolder(), delegateTypes));
        }

        private static IEnumerable<FunctionExportProvider> CreateExportProviders(IEnumerable<Assembly> assemblies, params Type[] delegateTypes)
        {
            return assemblies.Select(a => new FunctionExportProvider(a, delegateTypes));
        }

        private static IEnumerable<Assembly> LoadAssembliesInFolder()
        {
            return Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll")
                .Select(LoadAssemblyAndSuppressExceptions)
                .Where(assembly => assembly != null);
        }

        private static Assembly LoadAssemblyAndSuppressExceptions(string file)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(file);
            }
            catch (ArgumentNullException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (FileLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (PathTooLongException)
            {
            }
            return assembly;
        }
    }
}
