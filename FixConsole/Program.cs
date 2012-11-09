using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Fix;
using TestServer;

namespace FixUp
{
    class Program
    {
        static void Main(string[] args)
        {
            var prefix = args.Length == 1 ? args[0] : "http://*:3333/";
            using (var server = new Server(prefix))
            {
                var fixer = new Fixer(func => server.Start(func), server.Stop);

                using (var catalog = new DirectoryCatalog(Environment.CurrentDirectory))
                {
                    var container = new CompositionContainer(catalog);
                    container.ComposeParts(fixer);
                }

                fixer.Start();
                Console.Write("Running. Press Enter to stop.");
                Console.ReadLine();
                fixer.Stop();
            }
        }
    }
}
