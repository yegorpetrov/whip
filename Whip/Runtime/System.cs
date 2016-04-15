using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whip.Scripting;
using Whip.Widgets;

namespace Whip.Runtime
{
    partial class System
    {
        public readonly static System Instance = new System();

        static readonly IDictionary<Guid, Type> knownTypes = new Dictionary<Guid, Type>()
        {
            { new Guid("D6F50F64-93FA-49b7-93F1-BA66EFAE3E98"), typeof(System) },
            { new Guid("E90DC47B-840D-4ae7-B02C-040BD275F7FC"), typeof(Container) },
            { new Guid("4EE3E199-C636-4bec-97CD-78BC9C8628B0"), typeof(GuiObject) },
            { new Guid("45BE95E5-2072-4191-935C-BB5FF9F117FD"), typeof(Group) },
            { new Guid("5AB9FA15-9A7D-4557-ABC8-6557A6C67CA9"), typeof(Layer) },
            { new Guid("698EDDCD-8F1E-4fec-9B12-F944F909FF45"), typeof(WButton) },
            //{ new Guid(""), typeof() },
            //{ new Guid(""), typeof() },
            //{ new Guid(""), typeof() },
            //{ new Guid(""), typeof() },
            //{ new Guid(""), typeof() },
            //{ new Guid(""), typeof() },
            //{ new Guid(""), typeof() },
            //{ new Guid(""), typeof() },
            //{ new Guid(""), typeof() },
        };

        public object GetStaticObject(Guid guid)
        {
            if (ResolveType(guid) == typeof(System)) return this;
            else return null;
        }

        public Type ResolveType(Guid g)
        {
            Type result;
            knownTypes.TryGetValue(g, out result);
            if (result == null)
            {
                Debug.WriteLine("Can't resolve " + g);
            }
            return result;
        }

        public VCPU LoadScript(byte[] code, Group group)
        {
            return new VCPU(code, new GroupContextProxy(group));
        }

        class GroupContextProxy : IScriptContext
        {
            public readonly Group Group;

            public GroupContextProxy(Group group)
            {
                Group = group;
            }

            public object GetStaticObject(Guid guid)
            {
                return Instance.GetStaticObject(guid);
            }

            public Type ResolveType(Guid g)
            {
                return Instance.ResolveType(g);
            }
        }
    }
}
