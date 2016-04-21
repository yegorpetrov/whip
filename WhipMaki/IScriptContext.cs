using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
