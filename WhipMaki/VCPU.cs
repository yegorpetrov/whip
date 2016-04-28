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
        public readonly IScriptContext Context;
        readonly IReadOnlyList<Type> types;
        readonly IReadOnlyList<ScriptImport> imports;
        readonly byte[] code;
        readonly Action shutdown;
        
        readonly ScriptStack stack;
        
        public VCPU(byte[] exe, IScriptContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }
            Context = ctx;

            maki = new Maki(exe);
            types = maki.Guids.Select(
                g => ctx.ResolveType(g)).ToList();
            imports = maki.Imports.Select(
                i => new ScriptImport(ctx, i)).ToList();
            var events = maki.Listeners.Select(
                l => new ScriptEvent(ctx, l, CreateEventHandler)).ToList();
            var memory = new ScriptMemory(
                maki.Objects.Select(InstantiateIfGuid), events);
            stack = new ScriptStack(memory);
            code = maki.Code.ToArray();

            // We have init section
            ExecuteAndStop(0, maki.Listeners.Min(l => l.Offset));

            shutdown = memory.Unsubscribe;
        }

        object InstantiateIfGuid(object o)
        {
            return o is Guid ? Context.GetStaticObject((Guid)o) : o;
        }

        public void Shutdown()
        {
            shutdown();
        }

        object CallImportN(int n, ArgPuller arg, int nargs)
        {
            return imports[n].Call(arg, nargs);
        }

        object CreateInstance(int n)
        {
            return Activator.CreateInstance(Context.ResolveType(maki.Guids[n]));
        }

        public object Run(int offset, params object[] args)
        {
            args.ForEach(a => stack.Push(a));
            return Execute(offset);
        }

        object Execute(int offset) => ExecuteAndStop(offset, int.MaxValue);

        object ExecuteAndStop(int offset, int stop)
        {
            var address = new ScriptSequencer(CallImportN, CreateInstance, code, offset);
            while(address.PC < stop)
            {
                var opcode = address.Arg8();
                if (opcode == (byte)OPC.ret)
                {
                    if (address.Return())
                    {
                        return stack.Pop();
                    }
                }
                else ops[opcode](address, stack);
            }
            return null;
        }

        delegate void Cycle(ScriptSequencer seq, ScriptStack st);

        static readonly Cycle[] ops = new Cycle[byte.MaxValue];

        static VCPU()
        {
            new Dictionary<OPC, Cycle>()
            {
                { OPC.nop, (seq, st) => { } },
                { OPC.load, (seq, st) => st.Load(seq.Arg32()) },
                { OPC.drop, (seq, st) => st.Pop() },
                { OPC.save, (seq, st) => st.Save(seq.Arg32()) },
                { OPC.cmpeq, (seq, st) => st.Pop2Push1((a, b) => a == b) },
                { OPC.cmpne, (seq, st) => st.Pop2Push1((a, b) => a != b) },
                { OPC.cmpgt, (seq, st) => st.Pop2Push1((a, b) => a < b) },
                { OPC.cmpge, (seq, st) => st.Pop2Push1((a, b) => a <= b) },
                { OPC.cmplt, (seq, st) => st.Pop2Push1((a, b) => a > b) },
                { OPC.cmple, (seq, st) => st.Pop2Push1((a, b) => a >= b) },
                { OPC.jiz, (seq, st) => seq.Jump(!Convert.ToBoolean(st.Pop())) },
                { OPC.jnz, (seq, st) => seq.Jump(Convert.ToBoolean(st.Pop())) },
                { OPC.jmp, (seq, st) => seq.Jump(true) },
                { OPC.climp, (seq, st) => st.Push(seq.callImportN(seq.Arg32(), st.Pop, -1)) },
                { OPC.climpn, (seq, st) => st.Push(seq.callImportN(seq.Arg32(), st.Pop, seq.Arg8())) },
                { OPC.clint, (seq, st) => seq.CallFunction() },
                { OPC.stop, (seq, st) => { } },
                { OPC.set, (seq, st) => { st.Pop2Push1((p1, p2) => p1); st.SaveTop(); } },
                { OPC.incp, (seq, st) => { st.Pop1Push1(n1 => n1 + 1); st.SaveTop(); } },
                { OPC.decp, (seq, st) => { st.Pop1Push1(n1 => n1 - 1); st.SaveTop(); } },
                { OPC.pinc, (seq, st) => { st.Pop1Push1(n1 => n1 + 1); st.SaveTop(); } },
                { OPC.pdec, (seq, st) => { st.Pop1Push1(n1 => n1 - 1); st.SaveTop(); } },
                { OPC.add, (seq, st) => { st.Pop2Push1((a, b) => b + a); } },
                { OPC.sub, (seq, st) => { st.Pop2Push1((a, b) => b - a); } },
                { OPC.mul, (seq, st) => { st.Pop2Push1((a, b) => b * a); } },
                { OPC.div, (seq, st) => { st.Pop2Push1((a, b) => b / a); } },
                { OPC.mod, (seq, st) => { st.Pop2Push1((a, b) => b % a); } },
                { OPC.band, (seq, st) => { st.Pop2Push1((a, b) => a & b); } },
                { OPC.bor, (seq, st) => { st.Pop2Push1((a, b) => a | b); } },
                { OPC.not, (seq, st) => { st.Pop1Push1(a => !Convert.ToBoolean(a)); } },
                { OPC.bnot, (seq, st) => { st.Pop1Push1(a => ~a); } },
                { OPC.neg, (seq, st) => { st.Pop1Push1(a => -a); } },
                { OPC.bxor, (seq, st) => { st.Pop2Push1((a, b) => a ^ b); } },
                { OPC.and, (seq, st) => { st.Pop2Push1((a, b) => a && b); } },
                { OPC.or, (seq, st) => { st.Pop2Push1((a, b) => a || b); } },
                { OPC.shl, (seq, st) => { st.Pop2Push1((a, b) => b << a); } },
                { OPC.shr, (seq, st) => { st.Pop2Push1((a, b) => b >> a); } },
                { OPC.make, (seq, st) => st.Push(seq.createInstance(seq.Arg32())) },
                { OPC.del, (seq, st) => st.DeleteTop() }
            }
            .ForEach(kv => ops[(byte)kv.Key] = kv.Value);
        }
    }
}
