using System;
using System.Reflection;

namespace WhipMaki
{
    class ScriptEvent
    {
        public delegate Delegate HandlerMaker(EventInfo evi, int offset);

        readonly EventInfo @event;
        readonly IScriptContext context;
        readonly Delegate handler;
        public readonly int objectIndex;

        public ScriptEvent(IScriptContext ctx,
            Maki.Listener listener, HandlerMaker hm)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }
            context = ctx;
            var import = listener.Maki.Imports[listener.CallIdx];
            var guid = listener.Maki.Guids[import.TypeIdx];

            @event = ResolveEvent(ctx.ResolveType(guid), import.Name);
            handler = hm(@event, listener.Offset);
            objectIndex = listener.ObjIdx;
        }

        public void Subscribe(object o)
        {
            if (o != null)
            {
                @event.AddEventHandler(o, handler);
            }
        }

        public void Unsubscribe(object o)
        {
            if (o != null)
            {
                @event.RemoveEventHandler(o, handler);
            }
        }

        static EventInfo ResolveEvent(Type t, string name)
        {
            var flags =
                BindingFlags.IgnoreCase |
                BindingFlags.Public |
                BindingFlags.Instance;

            return
                t?.GetEvent(name, flags) ??
                t?.GetEvent(Remove_On_FromName(name), flags);
        }

        static string Remove_On_FromName(string name)
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
