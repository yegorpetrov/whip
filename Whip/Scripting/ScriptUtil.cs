using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    public static class ScriptUtil
    {
        public static void Disassemble(IDictionary<int, string> functions, IEnumerable<string> imports, string name, byte[] code, int offset = 0, int stop = int.MaxValue)
        {
            Func<int> arg32 = () =>
            {
                var result = BitConverter.ToInt32(code, offset);
                offset += 4;
                return result;
            };

            Func<byte> arg8 = () =>
            {
                var result = code[offset];
                offset++;
                return result;
            };

            name = name ?? "F" + offset;
            var listing = new StringBuilder(name + Environment.NewLine);

            if (functions.ContainsKey(offset)) return;
            var jumps = new Dictionary<int, string>();
            VCPU.opc opc;
            while (offset < code.Length && offset < stop)
            {
                var args = string.Empty;
                if (jumps.ContainsKey(offset))
                {
                    listing.AppendLine();
                    listing.Append("  " + jumps[offset] + ":\t");
                    jumps.Remove(offset);
                }
                else
                {
                    listing.Append("  " + offset + ":\t");
                }

                var @break = false;
                switch (opc = (VCPU.opc)code[offset++])
                {
                    case VCPU.opc.load:
                    case VCPU.opc.save:
                    case VCPU.opc.del:
                        args = "v" + arg32();
                        break;
                    case VCPU.opc.make:
                        args = "t" + arg32();
                        break;
                    case VCPU.opc.jiz:
                    case VCPU.opc.jmp:
                    case VCPU.opc.jnz:
                        {
                            var jmp = arg32() + offset;
                            if (jmp < offset)
                            {
                                listing.Replace(jmp + ":", "L" + jmp + ":");
                            }
                            else
                            {
                                jumps[jmp] = "L" + jmp;
                            }
                            args = "L" + jmp;
                        }
                        break;
                    case VCPU.opc.ret:
                        @break = jumps.IsEmpty();
                        break;
                    case VCPU.opc.clint:
                        {
                            var call = arg32() + offset;
                            args = "F" + call;
                            Disassemble(functions, imports, args, code, call);
                        }
                        break;
                    case VCPU.opc.climp:
                    case VCPU.opc.climpn:
                        {
                            var import = arg32();
                            args = imports.ElementAtOrDefault(import) ?? import.ToString(CultureInfo.InvariantCulture);
                            if (opc == VCPU.opc.climpn)
                            {
                                args = arg8() + " " + args;
                            }
                        }
                        break;
                }
                listing.AppendLine(string.Join(" ", opc.ToString(), args));
                if (@break)
                {
                    listing.AppendLine(";" + name);
                    break;
                }
            }
            if (jumps.Any())
            {
                throw new InvalidProgramException();
            }
            functions[offset] = listing.ToString();
        }
    }
}
