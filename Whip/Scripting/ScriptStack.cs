using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    class ScriptStack : Stack<dynamic>
    {
        ScriptMemory objects;

        public ScriptStack(ScriptMemory objectStore)
        {
            objects = objectStore;
        }

        readonly IDictionary<int, int> positions = new Dictionary<int, int>();

        public void Pop2Push1(Func<dynamic, dynamic, dynamic> f)
        {
            Push(f(Pop(), Pop()));
        }

        public void Pop1Push1(Func<dynamic, dynamic> f)
        {
            Push(f(Pop()));
        }

        public void Load(int idx)
        {
            Push(objects[positions[Count + 1] = idx]);
        }

        public void SaveTop()
        {
            objects[positions[Count]] = Peek();
        }

        public void Save(int i)
        {
            objects[i] = Pop();
        }

        public void DeleteTop()
        {
            (Pop() as IDisposable)?.Dispose();
            Push(null);
        }
    }
}
