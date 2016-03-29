using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    public class VCPU
    {
        readonly ScriptContext ctx;
        readonly Type[] types;
        readonly string[] calls;
        readonly MethodInfo[] methods;
        readonly EventInfo[] events;
        readonly object[] objects;
        readonly byte[] code;

        readonly ScriptStack<dynamic> stack = new ScriptStack<dynamic>();
        
        public VCPU(byte[] exe, ScriptContext ctx)
        {
            this.ctx = ctx;
            using (var reader = new ScriptReader(exe))
            {
                reader.ReadHeader();
                var guids = reader.ReadTypeTable().ToArray();
                calls = reader.ReadCallTable().Select(t => t.Item2).ToArray();
                objects = reader.ReadObjectTable(guids).ToArray();
                reader.ReadStringTable().ForEach(s => objects[s.Item1] = s.Item2);
                var listeners = reader.ReadListenerTable().ToArray();
                code = reader.ReadBytecode();

                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] is Guid) objects[i] = ctx.GetStaticObject((Guid)objects[i]);
                }

                //types = ctx.ResolveTypes(guids).ToArray();
                //var flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
                //methods = calls.Select(c => types[c.Item1].GetMethod(c.Item2, flags)).ToArray();
                //events = calls.Select(c => types[c.Item1].GetEvent(c.Item2, flags)).ToArray();
            }
        }

        public void Run()
        {
            Execute(0);
        }

        void Execute(int offset)
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

            while (true)
            {
                switch ((opc)code[offset])
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
                    case opc.jiz: var n = Convert.ToBoolean(stack.Pop()) ? 0 : arg32(); offset += n; break;
                    case opc.jnz: n = Convert.ToBoolean(stack.Pop()) ? arg32() : 0; offset += n; break;
                    case opc.jmp: offset += arg32(); break;
                    case opc.callext: throw new NotImplementedException();
                    case opc.callint: throw new NotImplementedException();
                    case opc.callext2:
                        {
                            var call = calls[arg32()];
                            var nargs = arg8();
                            var args = EnumerableEx.Generate(0, i => i < nargs, i => i + 1, i => stack.Pop()).ToArray();
                            stack.Push((stack.Pop() as IScriptable).ScriptMethod(call, args)); break;
                        }
                    case opc.ret: return;
                    case opc.stop: break; //TODO
                    case opc.set: stack.SetRHS(objects); break;
                    case opc.incs: throw new NotImplementedException();
                    case opc.decs: throw new NotImplementedException();
                    case opc.incp: throw new NotImplementedException();
                    case opc.decp: throw new NotImplementedException();
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
                    case opc.inst: throw new NotImplementedException();
                    case opc.del: throw new NotImplementedException();
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
