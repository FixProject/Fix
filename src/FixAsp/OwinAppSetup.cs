using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace FixAsp
{
    using Print;
    using UseAction = Action<Func<IDictionary<string, object>, Func<Task>, Task>>;

    public static class OwinAppSetup
    {
        public static void Setup(UseAction use)
        {
            var requestPrinter = new RequestPrinter();
            use(requestPrinter.PrintRequest);
        }
    }
}