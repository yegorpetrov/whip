using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    public partial class VCPU
    {
        const BindingFlags CallFlags =
            BindingFlags.IgnoreCase |
            BindingFlags.Public |
            BindingFlags.Instance;

        readonly IScriptContext ctx;
        readonly Type[] types;
        readonly MethodInfo[] methods;
        readonly ScriptObjectStore objects;
        readonly byte[] code;

        readonly ScriptStack<dynamic> stack = new ScriptStack<dynamic>();
        
        public VCPU(byte[] exe, IScriptContext ctx)
        {
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

                objects = new ScriptObjectStore(
                    reader.ReadObjectTable(guids).ToArray(),
                    reader.ReadStringTable().ToArray());

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
                    .Select(c =>
                    {
                        var mi = types[c.TypeIdx]?.GetMethod(c.Name, CallFlags) ??
                                types[c.TypeIdx]?.GetMethod(
                                    ScriptUtil.TranslateGetterSetter(c.Name), CallFlags);
                        if (mi == null && !c.Name.StartsWith("on"))
                        {
                            Debug.WriteLine("Null MI: T{0}, C='{1}'", c.TypeIdx, c.Name);
                        }
                        return mi;
                    })
                    .ToArray();

                objects.CreateListeners(listeners.Select(l =>
                {
                    var typeIdx = calls[l.Call].TypeIdx;
                    var callName = calls[l.Call].Name;
                    var evi = types[typeIdx]?.GetEvent(callName, CallFlags) ??
                        types[typeIdx]?.GetEvent(
                            ScriptUtil.TranslateEvent(callName), CallFlags);
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

        public void Run()
        {
            Execute(0);
        }

        void Execute(int offset) => ExecuteAndStop(offset, int.MaxValue);

        void ExecuteAndStop(int offset, int stop)
        {
            Func<int> arg32 = () =>
            {
                var result = BitConverter.ToInt32(code, offset + 1);
                offset += 4;
                return result;
            };

            Func<byte> arg8 = () =>
            {
                var result = code[offset + 1];
                offset++;
                return result;
            };

            opc op;
            while (offset < stop)
            {
                switch (op = (opc)code[offset])
                {
                    case opc.nop: break;
                    case opc.load: stack.Load(objects, arg32()); break;
                    case opc.drop: stack.Pop(); break;
                    case opc.save: objects[arg32()] = stack.Pop(); break;
                    case opc.cmpeq: stack.Pop2Push1((a, b) => a == b); break;
                    case opc.cmpne: stack.Pop2Push1((a, b) => a != b); break;
                    case opc.cmpgt: stack.Pop2Push1((a, b) => a < b); break;
                    case opc.cmpge: stack.Pop2Push1((a, b) => a <= b); break;
                    case opc.cmplt: stack.Pop2Push1((a, b) => a > b); break;
                    case opc.cmple: stack.Pop2Push1((a, b) => a >= b); break;
                    case opc.jiz:
                    case opc.jnz:
                        {
                            var sw = Convert.ToBoolean(stack.Pop());
                            var jmp = arg32();
                            if (op == opc.jnz) offset += sw ? jmp : 0;
                            else offset += sw ? 0 : jmp;
                        }
                        break;
                    case opc.jmp:
                        {
                            var jmp = arg32();
                            offset += jmp;
                        }
                        break;
                    case opc.climp:
                        {
                            var call = methods[arg32()];
                            CallExt(call, -1);
                        }
                        break;
                    case opc.clint:
                        {
                            var f = arg32();
                            ExecuteAndStop(f + offset + 1, int.MaxValue);
                        }
                        break;
                    case opc.climpn:
                        {
                            var call = methods[arg32()];
                            var nargs = arg8();
                            CallExt(call, nargs);
                        }
                        break;
                    case opc.ret: return;
                    case opc.stop: break; //TODO
                    case opc.set: stack.Pop2Push1((p1, p2) => p1); stack.SaveTop(objects); break;
                    case opc.incp: stack.Pop1Push1(n1 => n1 + 1); stack.SaveTop(objects); break;
                    case opc.decp: stack.Pop1Push1(n1 => n1 - 1); stack.SaveTop(objects); break;
                    case opc.pinc: stack.Pop1Push1(n1 => n1 + 1); stack.SaveTop(objects); break;
                    case opc.pdec: stack.Pop1Push1(n1 => n1 - 1); stack.SaveTop(objects); break;
                    case opc.add: stack.Pop2Push1((a, b) => b + a); break;
                    case opc.sub: stack.Pop2Push1((a, b) => b - a); break;
                    case opc.mul: stack.Pop2Push1((a, b) => b * a); break;
                    case opc.div: stack.Pop2Push1((a, b) => b / a); break;
                    case opc.mod: stack.Pop2Push1((a, b) => b % a); break;
                    case opc.band: stack.Pop2Push1((a, b) => a & b); break;
                    case opc.bor: stack.Pop2Push1((a, b) => a | b); break;
                    case opc.not: stack.Pop1Push1(a => !Convert.ToBoolean(a)); break;
                    case opc.bnot: stack.Pop1Push1(a => ~a); break;
                    case opc.neg: stack.Pop1Push1(a => -a); break;
                    case opc.bxor: stack.Pop2Push1((a, b) => a ^ b); break;
                    case opc.and: stack.Pop2Push1((a, b) => a && b); break;
                    case opc.or: stack.Pop2Push1((a, b) => a || b); break;
                    case opc.shl: stack.Pop2Push1((a, b) => b << a); break;
                    case opc.shr: stack.Pop2Push1((a, b) => b >> a); break;
                    case opc.make: stack.Push(Activator.CreateInstance(types[arg32()])); break;
                    case opc.del: stack.DeleteTop(objects); break;
                    default: throw new NotImplementedException();
                }
                offset++;
            }
        }

        private void CallExt(MethodInfo call, int nargs)
        {
            var needsContext = call.GetCustomAttribute(typeof(NeedsContextAttribute)) != null;
            if (nargs < 0)
            {
                nargs = call.GetParameters().Count() - (needsContext ? 1 : 0);
            }

            var args = EnumerableEx.Generate(0, i => i < nargs, i => i + 1, i => stack.Pop());
            var expected = call.GetParameters().Select(p => p.ParameterType);
            args =
                EnumerableEx.If(
                    () => call.GetCustomAttribute(typeof(NeedsContextAttribute)) != null,
                    EnumerableEx.Return(ctx))
                .Concat(
                    args
                    .Zip(expected, (a, b) => (a is int && b == typeof(bool)) ? Convert.ToBoolean(a) : a))
                 .ToArray();
            stack.Pop1Push1(t => call.Invoke(t, args.ToArray()));
        }

        void DoCallExt()
        {

        }

        internal enum opc : byte
        {
            nop = 0x0,
            load = 0x1, // int32 obj_idx
            drop = 0x2,
            save = 0x3, // int32 obj_idx
            cmpeq = 0x8,
            cmpne = 0x9,
            cmpgt = 0xa,
            cmpge = 0xb,
            cmplt = 0xc,
            cmple = 0xd,
            jiz = 0x10, // int32 offset
            jnz = 0x11, // int32 offset
            jmp = 0x12, // int32 offset
            climp = 0x18, // int32 import
            clint = 0x19, // int32 offset
            ret = 0x21,
            stop = 0x28,
            set = 0x30,
            incp = 0x38, // A++
            decp = 0x39, // A−−
            pinc = 0x3a, // ++A
            pdec = 0x3b, // −−A
            add = 0x40,
            sub = 0x41,
            mul = 0x42,
            div = 0x43,
            mod = 0x44,
            band = 0x48, // A & B
            bor = 0x49, // A | B
            not = 0x4a, // !A
            bnot = 0x4b, // ~A
            neg = 0x4c, // −1*A
            bxor = 0x4d, // A ^ B
            and = 0x50, // A && B
            or = 0x51, // A || B
            shl = 0x58,
            shr = 0x59,
            make = 0x60, // int32 type
            del = 0x61, // delete
            climpn = 0x70, // int32 import, int8 n_args
        }
    }
}
