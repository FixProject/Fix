using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNowinDemo
{
    using Fix;
    using Nowin;
    using Simple.Web;
    using Simple.Web.Behaviors;

    class Program
    {
        static void Main()
        {
            var fixer = new Fixer();
            fixer.Use((e, t) => Application.Run(e));

            var builder = ServerBuilder.New().SetPort(1337).SetOwinApp(fixer.Build());
            using (builder.Start())
            {
                Console.WriteLine("Listening on port 1337. Enter to exit.");
                Console.ReadLine();
            }
        }
    }

    [UriTemplate("/")]
    public class Index : IGet, IOutput<RawHtml>
    {
        public Status Get()
        {
            return 200;
        }

        public RawHtml Output
        {
            get { return "<h1>Ta dah!</h1>"; }
        }
    }
}
