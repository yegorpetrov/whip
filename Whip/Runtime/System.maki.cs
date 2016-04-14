using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whip.Scripting;
using Whip.Widgets;

namespace Whip.Runtime
{
    partial class System
    {
        public event Action Main;

        [NeedsContext]
        public Group GetScriptGroup(IScriptContext ctx)
        {
            return (ctx as GroupContextProxy)?.Group;
        }
    }
}
