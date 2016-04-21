using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    class ScriptImport
    {
        readonly int nargs;
        readonly MethodInfo method;
        readonly bool needsContext;
        readonly IScriptContext context;
        readonly Type[] parameterTypes;
        readonly IList<object> args = new List<object>();

        public ScriptImport(IScriptContext ctx, Guid type, string name)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }
            context = ctx;
            method = ResolveImport(ctx.ResolveType(type), name);
            parameterTypes = method?.GetParameters().Select(p => p.ParameterType).ToArray();
            needsContext = method?.GetCustomAttribute<NeedsContextAttribute>() != null;
            nargs = method?.GetParameters().Count() ?? 0;
        }

        public object Call(ArgPuller argf, int nargs)
        {
            if (nargs < 0) nargs = parameterTypes.Length;
            args.Clear();
            if (needsContext) args.Add(context);
            for (int i = 0; i < nargs; i++)
            {
                var arg = argf();
                args.Add(arg is int && parameterTypes[i] == typeof(bool) ?
                    Convert.ToBoolean(arg) : arg);
            }
            return method.Invoke(argf(), args.ToArray());
        }

        const BindingFlags CallFlags =
            BindingFlags.IgnoreCase |
            BindingFlags.Public |
            BindingFlags.Instance;

        static MethodInfo ResolveImport(Type t, string name)
        {
            return
                t?.GetMethod(name, CallFlags) ??
                t?.GetMethod(TranslateGetterSetter(name), CallFlags);
        }

        static string TranslateGetterSetter(string getSetName)
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
    }
}
