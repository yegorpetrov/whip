using System;
using System.Collections.Generic;
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

        readonly ScriptContext ctx;
        readonly Type[] types;
        readonly MethodInfo[] methods;
        readonly ScriptObjectStore objects;
        readonly byte[] code;

        readonly ScriptStack<dynamic> stack = new ScriptStack<dynamic>();
        
        public VCPU(byte[] exe, ScriptContext ctx)
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
                        types[c.TypeIdx].GetMethod(c.Name, CallFlags) ??
                        types[c.TypeIdx].GetMethod(
                            ScriptUtil.TranslateGetterSetter(c.Name), CallFlags))
                    .ToArray();

                objects.CreateListeners(listeners.Select(l =>
                {
                    var typeIdx = calls[l.Call].TypeIdx;
                    var callName = calls[l.Call].Name;
                    var evi = types[typeIdx].GetEvent(callName, CallFlags) ??
                        types[typeIdx].GetEvent(
                            ScriptUtil.TranslateEvent(callName), CallFlags);
                    return new Tuple<int, EventInfo, Delegate>(
                        l.Obj, evi, CreateEventHandler(evi, l.Offset, this)
                        );
                }));

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
                    case opc.push: stack.PushN(objects, arg32()); break;
                    case opc.popi: stack.Pop(); break;
                    case opc.pop: objects[arg32()] = stack.Pop(); break;
                    case opc.cmpeq: stack.Pop2Push1((a, b) => a.Equals(b)); break;
                    case opc.cmpne: stack.Pop2Push1((a, b) => !a.Equals(b)); break;
                    case opc.cmpg: stack.Pop2Push1((a, b) => a < b); break;
                    case opc.cmpge: stack.Pop2Push1((a, b) => a <= b); break;
                    case opc.cmpl: stack.Pop2Push1((a, b) => a > b); break;
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
                    case opc.callext:
                        {
                            var call = methods[arg32()];
                            var nargs = call.GetParameters().Count();
                            var args = EnumerableEx.Generate(0, i => i < nargs, i => i + 1, i => stack.Pop());
                            var expected = call.GetParameters().Select(p => p.ParameterType);
                            args = args.Zip(expected, (a, b) => (a is int && b == typeof(bool)) ? Convert.ToBoolean(a) : a).ToArray();
                            stack.Pop1Push1(t => call.Invoke(t, args.ToArray()));
                        }
                        break;
                    case opc.callint:
                        {
                            var f = arg32();
                            ExecuteAndStop(f + offset + 1, int.MaxValue);
                        }
                        break;
                    case opc.callext2:
                        {
                            var call = methods[arg32()];
                            var nargs = arg8();
                            var args = EnumerableEx.Generate(0, i => i < nargs, i => i + 1, i => stack.Pop());
                            var expected = call.GetParameters().Select(p => p.ParameterType);
                            args = args.Zip(expected, (a, b) => (a is int && b == typeof(bool)) ? Convert.ToBoolean(a) : a).ToArray();
                            stack.Pop1Push1(t => call.Invoke(t, args.ToArray()));
                        }
                        break;
                    case opc.ret: return;
                    case opc.stop: break; //TODO
                    case opc.set: stack.Pop2Push1((p1, p2) => p1); stack.SaveTop(objects); break;
                    case opc.incs: stack.Pop1Push1(n1 => n1 + 1); stack.SaveTop(objects); break;
                    case opc.decs: stack.Pop1Push1(n1 => n1 - 1); stack.SaveTop(objects); break;
                    case opc.incp: stack.Pop1Push1(n1 => n1 + 1); stack.SaveTop(objects); break;
                    case opc.decp: stack.Pop1Push1(n1 => n1 - 1); stack.SaveTop(objects); break;
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
                    case opc.inst: stack.Push(Activator.CreateInstance(types[arg32()])); break;
                    case opc.del: stack.DeleteTop(objects); break;
                    case opc.umv: throw new NotImplementedException();
                }
                offset++;
            }
        }

        internal enum opc : byte
        {
            nop = 0x0,
            push = 0x1, // Push a variable into stack
            popi = 0x2, // Pop a variable from the stack to nowhere
            pop = 0x3, // Pop a variable from the stack to object array
            cmpeq = 0x8, // A == B
            cmpne = 0x9, // A != B
            cmpg = 0xa, // A < B
            cmpge = 0xb, // A <= B
            cmpl = 0xc, // A > B
            cmple = 0xd, // A >= B
            jiz = 0x10, // Jump if zero
            jnz = 0x11, // Jump if non-zero
            jmp = 0x12, // Jump unconditionally
            callext = 0x18, // Call external (e.g. System.play())
            callint = 0x19, // Call internal (e.g. myFunc())
            callext2 = 0x70, // Same as cext but with 5 bytes long argument
            ret = 0x21, // return (value left in the stack)
            stop = 0x28, // Break event handling chain (“complete”)
            set = 0x30, // A = B
            incs = 0x38, // A++
            decs = 0x39, // A−−
            incp = 0x3a, // ++A
            decp = 0x3b, // −−A
            add = 0x40, // A + B
            sub = 0x41, // A − B
            mul = 0x42, // A * B
            div = 0x43, // A / B
            mod = 0x44, // A % B
            band = 0x48, // A & B
            bor = 0x49, // A | B
            not = 0x4a, // !A
            bnot = 0x4b, // Bitwise not
            neg = 0x4c, // −1*A
            bxor = 0x4d, // Bitwise xor
            and = 0x50, // A && B
            or = 0x51, // A || B
            shl = 0x58, // A << B
            shr = 0x59, // A >> B
            inst = 0x60, // instantiate
            del = 0x61, // delete
            umv = 0x68 // TODO: What's this?
        }
    }
}
