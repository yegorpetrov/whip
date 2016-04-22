using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhipMaki.Disassembly
{
    public class ListenerDisassembly : FunctionDisassembly
    {
        readonly string name;
        
        public ListenerDisassembly(Maki.Listener ml)
            : base(ml.Maki.Code.ToArray(), ml.Offset,
                  int.MaxValue, ml.Maki.Imports)
        {
            var import = ml.Maki.Imports[ml.CallIdx];
            name = string.Format("v{0}.{1}", ml.ObjIdx, import.Name);
        }

        public override string ToString()
        {
            return
                new StringBuilder(name)
                .AppendLine(base.ToString())
                .ToString();
        }
    }
}
