using System;
using Fix;
using Info;
using Print;
using TestModule;
using TestServer;

namespace FixConsole
{
    class Program
    {
        static void Main()
        {
            using (var server = new Server("http://*:81/"))
            {
                var fixer = new Fixer(server.Start, server.Stop);
                fixer.AddHandler(new RequestPrinter().PrintRequest);
                fixer.AddHandler(new InfoPrinter().PrintInfo);
                fixer.AddInfix(new MethodDownshifter().DownshiftMethod);
                fixer.Start();
                Console.Write("Running. Press Enter to stop.");
                Console.ReadLine();
                fixer.Stop();
            }
        }
    }
}
