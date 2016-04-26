using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhipMaki.Disassembly
{
    public class FunctionDisassembly : Disassembly
    {
        public readonly int startOffset;
        public readonly int stopOffset = int.MaxValue;
        public readonly byte[] code;
        readonly IEnumerable<Maki.Import> imports;

        public FunctionDisassembly(byte[] code0, int start, int stop,
            IEnumerable<Maki.Import> imports0)
        {
            startOffset = start;
            stopOffset = stop;
            code = code0;
            imports = imports0;
        }

        public override void Disassemble(IDictionary<int, Disassembly> funcs)
        {
            funcs[startOffset] = this;
            var offset = startOffset;

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

            listing.Clear();
            listing.AppendLine("@" + startOffset);
            var jumps = new Dictionary<int, string>();
            while (offset < stopOffset && offset < code.Length)
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
                OPC opc;
                switch (opc = (OPC)code[offset++])
                {
                    case OPC.load:
                    case OPC.save:
                        args = "v" + arg32();
                        break;
                    case OPC.make:
                        args = "t" + arg32();
                        break;
                    case OPC.jiz:
                    case OPC.jmp:
                    case OPC.jnz:
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
                    case OPC.ret:
                        @break = jumps.IsEmpty();
                        break;
                    case OPC.clint:
                        {
                            var call = arg32() + offset;
                            args = "F" + call;
                            if (!funcs.ContainsKey(call))
                            {
                                (funcs[call] = new FunctionDisassembly(code,
                                    call, int.MaxValue, imports)).Disassemble(funcs);
                            }
                        }
                        break;
                    case OPC.climp:
                    case OPC.climpn:
                        {
                            var import = arg32();
                            args =
                                imports.ElementAtOrDefault(import).Name ??
                                import.ToString(CultureInfo.InvariantCulture);

                            if (opc == OPC.climpn)
                            {
                                args = arg8() + " " + args;
                            }
                        }
                        break;
                }
                listing.AppendLine(string.Join(" ", opc.ToString(), args));
                if (@break)
                {
                    listing.AppendLine(";");
                    break;
                }
            }
            if (jumps.Any())
            {
                throw new Exception("Jumps left");
            }
        }
    }
}
