using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhipMaki.Disassembly
{
    public class ScriptDisassembly : Disassembly
    {
        Maki maki;

        public ScriptDisassembly(Maki m)
        {
            maki = m;
        }

        public override void Disassemble(IDictionary<int, Disassembly> funcs)
        {
            listing.Clear();
            funcs = funcs ?? new SortedDictionary<int, Disassembly>();
            maki
                .Listeners
                .Select(l => new ListenerDisassembly(l))
                .ForEach(l => l.Disassemble(funcs));

            var txt = string.Join("\r\n", funcs
                .Select(kv => kv.Value.ToString()));

            listing.Append(txt);
        }
    }
}
