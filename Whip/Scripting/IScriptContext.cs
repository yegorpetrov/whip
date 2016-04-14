using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    public interface IScriptContext
    {
        Type ResolveType(Guid g);
        object GetStaticObject(Guid guid);
    }
}
