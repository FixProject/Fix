using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CRack
{
    class Program
    {
        static void Main()
        {
            using (var cracker = new CRacker(new RequestProcessor().ProcessRequest))
            {
                cracker.Start();
                Console.Write("Running. Press Enter to stop.");
                Console.ReadLine();
                cracker.Stop();
            }
        }
    }
}
