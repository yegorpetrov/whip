using System.Collections.Generic;
using System.Linq;

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

            var min = maki.Listeners.Min(l => l.Offset);
            if (min > 0)
            {
                funcs[0] = new FunctionDisassembly(maki.Code.ToArray(), 0, min, maki.Imports);
                funcs[0].Disassemble(funcs);
            }

            var txt = string.Join("\r\n", funcs.Select(v => v.Value.ToString()));

            listing.Append(txt);
        }
    }
}
