using System;

namespace WhipMaki
{
    public interface IScriptContext
    {
        Type ResolveType(Guid g);
        object GetStaticObject(Guid guid);
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class NeedsContextAttribute : Attribute
    {
    }
}
