using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WhipMaki
{
    public delegate object ArgPuller();

    public partial class VCPU
    {
        const BindingFlags CallFlags =
            BindingFlags.IgnoreCase |
            BindingFlags.Public |
            BindingFlags.Instance;

        readonly Maki maki;
        readonly IScriptContext ctx;
        readonly IReadOnlyList<Type> types;
        readonly IReadOnlyList<ScriptImport> imports;
        readonly IReadOnlyCollection<ScriptEvent> events;
        readonly ScriptMemory memory;
        readonly byte[] code;
        
        readonly ScriptStack stack;
        
        public VCPU(byte[] exe, IScriptContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }
            this.ctx = ctx;

            maki = new Maki(exe);
            types = maki.Guids.Select(
                g => ctx.ResolveType(g)).ToList();
            imports = maki.Imports.Select(
                i => new ScriptImport(ctx, i)).ToList();
            events = maki.Listeners.Select(
                l => new ScriptEvent(ctx, l, CreateEventHandler)).ToList();
            memory = new ScriptMemory(
                maki.Objects.Select(InstantiateIfGuid), events);
            stack = new ScriptStack(memory);
            code = maki.Code.ToArray();

            // We have init section
            ExecuteAndStop(0, maki.Listeners.Min(l => l.Offset));
        }

        object InstantiateIfGuid(object o)
        {
            return o is Guid ? ctx.GetStaticObject((Guid)o) : o;
        }

        public void Unsubscribe()
        {
            memory.Unsubscribe();
        }

        object CallImportN(int n, ArgPuller arg, int nargs)
        {
            return imports[n].Call(arg, nargs);
        }

        object CreateInstance(int n)
        {
            return Activator.CreateInstance(ctx.ResolveType(maki.Guids[n]));
        }

        void Execute(int offset) => ExecuteAndStop(offset, int.MaxValue);

        void ExecuteAndStop(int offset, int stop)
        {
            var address = new ScriptSequencer(CallImportN, CreateInstance, code, offset);
            while(address.PC < stop)
            {
                var opcode = address.Arg8();
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

        static readonly Action<ScriptSequencer, ScriptStack>[] ops =
        new Action<ScriptSequencer, ScriptStack>[byte.MaxValue];

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
