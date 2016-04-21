using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WhipMaki
{
    class ScriptMemory : IEnumerable<object>
    {
        object[] objects;
        Tuple<int, EventInfo, Delegate>[] eventHandlers;

        public ScriptMemory(IEnumerable<object> objects, IEnumerable<Tuple<int, string>> strings)
        {
            this.objects = objects.ToArray();
            strings.ForEach(s => this.objects[s.Item1] = s.Item2);
        }

        public void CreateListeners(IEnumerable<Tuple<int, EventInfo, Delegate>> handlers)
        {
            eventHandlers = handlers.ToArray();
            for (int i = 0; i < objects.Length; i++) this[i] = objects[i];
        }

        public IEnumerator<object> GetEnumerator()
        {
            return objects.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public object this[int i]
        {
            get
            {
                return objects[i];
            }
            set
            {
                if (eventHandlers != null)
                { 
                    foreach (var h in eventHandlers.Where(h => h.Item1 == i && h.Item2 != null))
                    {
                        if (objects[i] != null) h.Item2.RemoveEventHandler(objects[i], h.Item3);
                        if (value != null) h.Item2.AddEventHandler(value, h.Item3);
                    }
                }
                objects[i] = value;
            }
        }

        public void Unsubscribe()
        {
            if (eventHandlers == null) return;
            foreach (var h in eventHandlers)
            {
                h.Item2.RemoveEventHandler(objects[h.Item1], h.Item3);
            }
        }
    }
}
