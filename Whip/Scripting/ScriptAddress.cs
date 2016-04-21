using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    class ScriptAddress
    {
        readonly Stack<int> callStack = new Stack<int>();
        public readonly Func<int, object> createInstance;
        public readonly Func<int, ArgPuller, int, object> callImportN;
        readonly byte[] code;
        int pc;

        public int PC
        {
            get { return pc; }
        }

        public ScriptAddress(
            Func<int, ArgPuller, int, object> nCaller,
            Func<int, object> newInstance,
            byte[] exe, int start)
        {
            if (nCaller == null)
            {
                throw new ArgumentNullException(nameof(nCaller));
            }
            if (newInstance == null)
            {
                throw new ArgumentNullException(nameof(newInstance));
            }
            callImportN = nCaller;
            code = exe;
            pc = start;
            createInstance = newInstance;
        }

        public byte Arg8()
        {
            return code[pc++];
        }

        public int Arg32()
        {
            var result = BitConverter.ToInt32(code, pc);
            pc += 4;
            return result;
        }

        public void Jump(bool jump)
        {
            int where = Arg32();
            if (jump)
            {
                pc += where;
            }
        }

        public void CallFunction()
        {
            callStack.Push(pc);
            Jump(true);
        }

        public bool Return()
        {
            if (callStack.IsEmpty())
            {
                return true;
            }
            else
            {
                pc = callStack.Pop();
                Jump(false);
                return false;
            }
        }

        public override string ToString()
        {
            return pc.ToString();
        }
    }
}
