using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhipMaki.Disassembly
{
    public abstract class Disassembly
    {
        readonly protected StringBuilder listing = new StringBuilder();
        public abstract void Disassemble(IDictionary<int, Disassembly> funcs);

        public override string ToString()
        {
            return listing.ToString();
        }
    }
}
