using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    partial class VCPU
    {
        static readonly MethodInfo
            _exec = typeof(VCPU).GetMethod(nameof(Execute),
                BindingFlags.NonPublic | BindingFlags.Instance),
            _push = typeof(ScriptStack<dynamic>).GetMethod("Push"),
            _pop = typeof(ScriptStack<dynamic>).GetMethod("Pop"),
            _enter = typeof(Monitor).GetMethod("Enter",
                BindingFlags.Public | BindingFlags.Static,
                Type.DefaultBinder, new[] { typeof(object) }, null),
            _exit = typeof(Monitor).GetMethod("Exit",
                BindingFlags.Public | BindingFlags.Static,
                Type.DefaultBinder, new[] { typeof(object) }, null);

        static readonly FieldInfo _stack = typeof(VCPU).GetField(
            nameof(stack), BindingFlags.NonPublic | BindingFlags.Instance);

        static Delegate CreateEventHandler(EventInfo evi, int offset, object target)
        {
            var handlerType = evi.EventHandlerType;

            var parTypes = EnumerableEx
                .Return(typeof(VCPU))
                .Concat(
                    handlerType
                    .GetMethod("Invoke")
                    .GetParameters()
                    .Select(p => p.ParameterType)
                )
                .ToArray();

            var retType = handlerType
                .GetMethod("Invoke")
                .ReturnType;

            var handler = new DynamicMethod(evi.Name + offset, retType, parTypes.ToArray(), typeof(VCPU));
            var gen = handler.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, _stack);
            gen.Emit(OpCodes.Call, _enter);

            parTypes
                .Skip(1)
                .Select((t, i) => new { t = t, i = i })
                .Reverse()
                .ForEach(pt =>
                    {
                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Ldfld, _stack);
                        gen.Emit(OpCodes.Ldarg, pt.i + 1);

                        var type = pt.t;
                        var eltype = type.GetElementType();
                        if (type.IsByRef)
                        {
                            if (eltype.IsValueType)
                            {
                                gen.Emit(OpCodes.Ldobj, eltype);
                                gen.Emit(OpCodes.Box, eltype);
                            }
                            else gen.Emit(OpCodes.Ldind_Ref);
                        }
                        else if (type.IsValueType)
                        {
                            gen.Emit(OpCodes.Box, type);
                        }
                        gen.Emit(OpCodes.Call, _push);
                    }
                );
            // Call “Execute” method
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4, offset);
            gen.Emit(OpCodes.Call, _exec);

            // Pop the value to return
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, _stack);
            gen.Emit(OpCodes.Callvirt, _pop);

            // MAKI pushes null onto stack even if the handler
            // must return void
            if (retType == typeof(void)) gen.Emit(OpCodes.Pop);
            else gen.Emit(OpCodes.Unbox_Any, retType);

            // Monitor.Exit(_opStack);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, _stack);
            gen.Emit(OpCodes.Call, _exit);

            // Return
            gen.Emit(OpCodes.Ret);

            return handler.CreateDelegate(handlerType, target);
        }
    }
}
