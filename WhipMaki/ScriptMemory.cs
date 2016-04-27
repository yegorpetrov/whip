using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WhipMaki
{
    class ScriptMemory : IEnumerable<object>
    {
        object[] objects;
        IEnumerable<ScriptEvent> listeners;

        public ScriptMemory(
            IEnumerable<object> objects,
            IEnumerable<ScriptEvent> listeners
            )
        {
            this.objects = objects.ToArray();
            this.listeners = listeners;

            this.listeners.ForEach(l => l.Subscribe(this.objects[l.objectIndex]));
        }

        public object this[int i]
        {
            get
            {
                return objects[i];
            }
            set
            {
                foreach (var h in listeners.Where(l => l.objectIndex == i))
                {
                    h.Unsubscribe(objects[i]);
                    h.Subscribe(value);
                }
                objects[i] = value;
            }
        }

        public void Unsubscribe()
        {
            listeners.ForEach(se => se.Unsubscribe(objects[se.objectIndex]));
        }

        public IEnumerator<object> GetEnumerator()
        {
            return objects.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
