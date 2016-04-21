using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WhipMaki;
using Whip.Widgets;

namespace Whip.Runtime
{
    class GroupContextProxy : IScriptContext
    {
        const BindingFlags CallFlags =
            BindingFlags.IgnoreCase |
            BindingFlags.Public |
            BindingFlags.Instance;

        public readonly Group Group;
        public readonly string Param;

        public GroupContextProxy(Group group, string param)
        {
            Group = group;
            Param = param;
        }

        public object GetStaticObject(Guid guid)
        {
            return System0.Instance.GetStaticObject(guid);
        }

        public Type ResolveType(Guid g)
        {
            return System0.Instance.ResolveType(g);
        }

        public EventInfo ResolveEvent(Type t, string name)
        {
            return
                t?.GetEvent(name, CallFlags) ??
                t?.GetEvent(TranslateEvent(name), CallFlags);
        }

        static string TranslateEvent(string name)
        {
            var cmp = StringComparison.InvariantCultureIgnoreCase;
            if (name.StartsWith("on", cmp))
            {
                return name.Substring(2);
            }
            else
            {
                return name;
            }
        }
    }
}
