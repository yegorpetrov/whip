using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    public interface IScriptable
    {
        event EventHandler<ScriptEventArgs> ScriptEvent;
        object ScriptMethod(string name, object[] args);
    }

    public class ScriptEventArgs : EventArgs
    {
        public ScriptEventArgs(string name, IEnumerable<object> args)
        {
            Name = name;
        }

        public string Name
        {
            get;
            private set;
        }

        public IEnumerable<object> Args
        {
            get;
            private set;
        }
    }
}
