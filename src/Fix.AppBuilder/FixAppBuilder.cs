using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;

namespace Fix.AppBuilder
{
    using AppFunc = Func<IDictionary<string,object>, Task>;

    class FixAppBuilder : IAppBuilder
    {
        private readonly IDictionary<string,object> _properties = new Dictionary<string, object>(); 
        private readonly Fixer _fixer;

        public FixAppBuilder(Fixer fixer)
        {
            _fixer = fixer;
        }

        public IAppBuilder Use(object middleware, params object[] args)
        {
            var properFunc = middleware as Func<AppFunc, AppFunc>;
            if (properFunc != null)
            {
                _fixer.Use(properFunc);
                return this;
            }
            throw new NotSupportedException("FixAppBuilder can't use that middleware.");
        }

        public object Build(Type returnType)
        {
            var output = _fixer.Build();
            if (output.GetType() != returnType)
            {
                throw new NotSupportedException("FixAppBuilder can't Build that Type.");
            }
            return output;
        }

        public IAppBuilder New()
        {
            return new FixAppBuilder(new Fixer());
        }

        public IDictionary<string, object> Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}