using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    class ScriptStack<T> : Stack<T> where T : class
    {
        readonly IDictionary<int, int> positions = new Dictionary<int, int>();

        public void Pop2Push1(Func<T, T, T> f)
        {
            Push(f(Pop(), Pop()));
        }

        public void Pop1Push1(Func<T, T> f)
        {
            Push(f(Pop()));
        }

        public void Load(ScriptObjectStore collection, int idx)
        {
            Push(collection[positions[Count + 1] = idx] as T);
        }

        public void SaveTop(ScriptObjectStore collection)
        {
            collection[positions[Count]] = Peek();
        }

        public void DeleteTop(ScriptObjectStore collection)
        {
            (Pop() as IDisposable)?.Dispose();
            Push(null);
        }
    }
}
