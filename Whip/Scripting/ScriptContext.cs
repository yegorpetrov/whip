using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    public abstract class ScriptContext
    {
        public abstract Type ResolveType(Guid g);
        public abstract object GetStaticObject(Guid guid);
    }
}
