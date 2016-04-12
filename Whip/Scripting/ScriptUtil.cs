using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    static class ScriptUtil
    {
        public static string TranslateGetterSetter(string getSetName)
        {
            var cmp = StringComparison.InvariantCultureIgnoreCase;
            if (getSetName.StartsWith("get", cmp) ||
                getSetName.StartsWith("set", cmp))
            {
                return getSetName.Insert(3, "_");
            }
            else
            {
                return getSetName;
            }
        }

        public static string TranslateEvent(string name)
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
