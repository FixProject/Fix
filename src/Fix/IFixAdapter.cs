namespace Fix
{
    using System;

    public interface IFixerAdapter
    {
        Type AdaptedType { get; }
        object Adapt(Fixer fixer);
    }
}