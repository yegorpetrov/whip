using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WhipMaki
{
    class ScriptImport
    {
        readonly int nargs;
        readonly MethodInfo method;
        readonly bool needsContext;
        readonly IScriptContext context;
        readonly IReadOnlyList<Type> parameterTypes;
        
        public ScriptImport(IScriptContext ctx, Maki.Import import)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }
            context = ctx;
            var guid = import.Maki.Guids[import.TypeIdx];
            method = ResolveImport(ctx.ResolveType(guid), import.Name);
            if (method != null)
            {
                parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                needsContext = method.IsDefined(typeof(NeedsContextAttribute));
                nargs = method.GetParameters().Count();
            }
        }

        public object Call(ArgPuller argf, int nargs)
        {
            var args = new List<object>();
            if (nargs < 0) nargs = parameterTypes.Count;
            args.Clear();
            if (needsContext)
            {
                args.Add(context);
                nargs--;
            }
            for (int i = 0; i < nargs; i++)
            {
                var arg = argf();
                args.Add(arg is int && parameterTypes[i] == typeof(bool) ?
                    Convert.ToBoolean(arg) : arg);
            }
            return method.Invoke(argf(), args.ToArray());
        }

        static MethodInfo ResolveImport(Type t, string name)
        {
            var flags =
                BindingFlags.IgnoreCase |
                BindingFlags.Public |
                BindingFlags.Instance;

            return
                t?.GetMethod(name, flags) ??
                t?.GetMethod(TranslateGetterSetter(name), flags);
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
