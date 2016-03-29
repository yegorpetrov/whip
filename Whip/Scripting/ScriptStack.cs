using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    class ScriptStack<T> : Stack<T>
    {
        readonly int[] index = new int[64];

        public void Pop2Push1(Func<T, T, T> f)
        {
            Push(f(Pop(), Pop()));
        }

        public void Pop1Push1(Func<T, T> f)
        {
            Push(f(Pop()));
        }

        public void PushN(T[] collection, int idx)
        {
            Push(collection[index[Count + 1] = idx]);
        }

        public void SetRHS(T[] collection)
        {
            var rhs = Pop();
            Pop();
            Push(rhs);
            collection[index[Count]] = rhs;
        }
    }
}
