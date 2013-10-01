namespace Fix.AppBuilder
{
    using System;
    using Owin;

    public class AppBuilderAdapter : IFixerAdapter
    {
        public Type AdaptedType
        {
            get { return typeof (IAppBuilder); }
        }

        public object Adapt(Fixer fixer)
        {
            return new FixAppBuilder(fixer);
        }
    }
}
