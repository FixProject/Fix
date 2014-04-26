using System;

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
            // Build the OWIN app
            var app = new Fixer()
                .Use(next => env => Application.Run(env, () => next(env)))
                .Build();

            // Set up the Nowin server
            var builder = ServerBuilder.New()
                .SetPort(31337)
                .SetOwinApp(app);

            // Run
            using (builder.Start())
            {
                Console.WriteLine("Listening on port 31337. Enter to exit.");
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
