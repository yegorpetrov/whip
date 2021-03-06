﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace WhipMaki
{
    partial class VCPU
    {
        static readonly MethodInfo
            _exec = typeof(VCPU).GetMethod(nameof(Execute),
                BindingFlags.NonPublic | BindingFlags.Instance),
            _push = typeof(ScriptStack).GetMethod("Push"),
            _pop = typeof(ScriptStack).GetMethod("Pop"),
            _enter = typeof(Monitor).GetMethod("Enter",
                BindingFlags.Public | BindingFlags.Static,
                Type.DefaultBinder, new[] { typeof(object) }, null),
            _exit = typeof(Monitor).GetMethod("Exit",
                BindingFlags.Public | BindingFlags.Static,
                Type.DefaultBinder, new[] { typeof(object) }, null);

        static readonly FieldInfo _stack = typeof(VCPU).GetField(
            nameof(stack), BindingFlags.NonPublic | BindingFlags.Instance);

        Delegate CreateEventHandler(EventInfo evi, int offset)
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

            return handler.CreateDelegate(handlerType, this);
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
