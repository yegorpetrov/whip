using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    public delegate object ArgPuller();

    public partial class VCPU
    {
        const BindingFlags CallFlags =
            BindingFlags.IgnoreCase |
            BindingFlags.Public |
            BindingFlags.Instance;

        readonly IScriptContext ctx;
        readonly Type[] types;
        readonly ScriptImport[] methods;
        readonly ScriptMemory objects;
        readonly byte[] code;

        readonly ScriptStack stack;
        
        public VCPU(byte[] exe, IScriptContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }
            this.ctx = ctx;
            using (var reader = new ScriptReader(exe))
            {
                reader.ReadHeader();

                var guids = reader
                    .ReadTypeTable()
                    .ToArray();

                var calls = reader
                    .ReadCallTable()
                    .Select(
                        c => new
                        {
                            TypeIdx = c.Item1,
                            Name = c.Item2
                        })
                    .ToArray();

                objects = new ScriptMemory(
                    reader.ReadObjectTable(guids).ToArray(),
                    reader.ReadStringTable().ToArray());

                stack = new ScriptStack(objects);

                for (int i = 0, _i = objects.Count(); i < _i; i++)
                {
                    if (objects[i] is Guid) objects[i] = ctx.GetStaticObject((Guid)objects[i]);
                }

                var listeners = reader
                    .ReadListenerTable()
                    .Select(
                        l => new
                        {
                            Obj = l.Item1,
                            Call = l.Item2,
                            Offset = l.Item3
                        })
                    .ToArray();

                code = reader.ReadBytecode();

                types = guids
                    .Select(ctx.ResolveType)
                    .ToArray();

                methods = calls
                    .Select(c => new ScriptImport(ctx, guids[c.TypeIdx], c.Name))
                    .ToArray();

                objects.CreateListeners(listeners.Select(l =>
                {
                    var typeIdx = calls[l.Call].TypeIdx;
                    var callName = calls[l.Call].Name;
                    var evi = types[typeIdx]?.GetEvent(callName, CallFlags) ??
                        types[typeIdx]?.GetEvent(
                            TranslateEvent(callName), CallFlags);
                    if (evi != null)
                    {
                        return new Tuple<int, EventInfo, Delegate>(
                        l.Obj, evi, CreateEventHandler(evi, l.Offset, this)
                        );
                    }
                    else
                    {
                        Debug.WriteLine("Null EVI: T{0} C '{1}'", typeIdx, callName);
                        return null;
                    }
                }).Where(l => l != null));

                // We have init section
                ExecuteAndStop(0, listeners.Min(l => l.Offset));
            }
        }

        public void Unsubscribe()
        {
            objects.Unsubscribe();
        }

        object CallImportN(int n, ArgPuller arg, int nargs)
        {
            return methods[n].Call(arg, nargs);
        }

        object CreateInstance(int n)
        {
            return Activator.CreateInstance(types[n]);
        }

        void Execute(int offset) => ExecuteAndStop(offset, int.MaxValue);

        void ExecuteAndStop(int offset, int stop)
        {
            var address = new ScriptAddress(CallImportN, CreateInstance, code, offset);
            byte opcode;
            while(true)
            {
                if (address.PC >= stop)
                {
                    break;
                }
                opcode = address.Arg8();
                if (opcode == (byte)OPC.ret)
                {
                    if (address.Return())
                    {
                        break;
                    }
                }
                else ops[opcode](address, stack);
            }
        }

        static readonly Action<ScriptAddress, ScriptStack>[] ops =
        new Action<ScriptAddress, ScriptStack>[byte.MaxValue];

        static VCPU()
        {
            Func<OPC, byte> _ = o => (byte)o;

            ops[_(OPC.nop)]  = (pc, stack) => { };
            ops[_(OPC.load)] = (pc, stack) => stack.Load(pc.Arg32());
            ops[_(OPC.drop)] = (pc, stack) => stack.Pop();
            ops[_(OPC.save)] = (pc, stack) => stack.Save(pc.Arg32());
            ops[_(OPC.cmpeq)] = (pc, stack) => stack.Pop2Push1((a, b) => a == b);
            ops[_(OPC.cmpne)] = (pc, stack) => stack.Pop2Push1((a, b) => a != b);
            ops[_(OPC.cmpgt)] = (pc, stack) => stack.Pop2Push1((a, b) => a < b);
            ops[_(OPC.cmpge)] = (pc, stack) => stack.Pop2Push1((a, b) => a <= b);
            ops[_(OPC.cmplt)] = (pc, stack) => stack.Pop2Push1((a, b) => a > b);
            ops[_(OPC.cmple)] = (pc, stack) => stack.Pop2Push1((a, b) => a >= b);
            ops[_(OPC.jiz)] = (pc, stack) => pc.Jump(!Convert.ToBoolean(stack.Pop()));
            ops[_(OPC.jnz)] = (pc, stack) => pc.Jump(Convert.ToBoolean(stack.Pop()));
            ops[_(OPC.jmp)] = (pc, stack) => pc.Jump(true);
            ops[_(OPC.climp)] = (pc, stack) => stack.Push(pc.callImportN(pc.Arg32(), stack.Pop, -1));
            ops[_(OPC.climpn)] = (pc, stack) => stack.Push(pc.callImportN(pc.Arg32(), stack.Pop, pc.Arg8()));
            ops[_(OPC.clint)] = (pc, stack) => pc.CallFunction();
            ops[_(OPC.stop)] = (pc, stack) => { };
            ops[_(OPC.set)] = (pc, stack) => { stack.Pop2Push1((p1, p2) => p1); stack.SaveTop(); };
            ops[_(OPC.incp)] = (pc, stack) => { stack.Pop1Push1(n1 => n1 + 1); stack.SaveTop(); };
            ops[_(OPC.decp)] = (pc, stack) => { stack.Pop1Push1(n1 => n1 - 1); stack.SaveTop(); };
            ops[_(OPC.pinc)] = (pc, stack) => { stack.Pop1Push1(n1 => n1 + 1); stack.SaveTop(); };
            ops[_(OPC.pdec)] = (pc, stack) => { stack.Pop1Push1(n1 => n1 - 1); stack.SaveTop(); };
            ops[_(OPC.add)] = (pc, stack) => { stack.Pop2Push1((a, b) => b + a); };
            ops[_(OPC.sub)] = (pc, stack) => { stack.Pop2Push1((a, b) => b - a); };
            ops[_(OPC.mul)] = (pc, stack) => { stack.Pop2Push1((a, b) => b * a); };
            ops[_(OPC.div)] = (pc, stack) => { stack.Pop2Push1((a, b) => b / a); };
            ops[_(OPC.mod)] = (pc, stack) => { stack.Pop2Push1((a, b) => b % a); };
            ops[_(OPC.band)] = (pc, stack) => { stack.Pop2Push1((a, b) => a & b); };
            ops[_(OPC.bor)] = (pc, stack) => { stack.Pop2Push1((a, b) => a | b); };
            ops[_(OPC.not)] = (pc, stack) => { stack.Pop1Push1(a => !Convert.ToBoolean(a)); };
            ops[_(OPC.bnot)] = (pc, stack) => { stack.Pop1Push1(a => ~a); };
            ops[_(OPC.neg)] = (pc, stack) => { stack.Pop1Push1(a => -a); };
            ops[_(OPC.bxor)] = (pc, stack) => { stack.Pop2Push1((a, b) => a ^ b); };
            ops[_(OPC.and)] = (pc, stack) => { stack.Pop2Push1((a, b) => a && b); };
            ops[_(OPC.or)] = (pc, stack) => { stack.Pop2Push1((a, b) => a || b); };
            ops[_(OPC.shl)] = (pc, stack) => { stack.Pop2Push1((a, b) => b << a); };
            ops[_(OPC.shr)] = (pc, stack) => { stack.Pop2Push1((a, b) => b >> a); };
            ops[_(OPC.make)] = (pc, stack) => stack.Push(pc.createInstance(pc.Arg32()));
            ops[_(OPC.del)] = (pc, stack) => stack.DeleteTop();
        }
    }
}
