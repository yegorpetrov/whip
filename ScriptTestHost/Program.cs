using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Whip.Scripting;

namespace ScriptTestHost
{
    class DummyCtx : IScriptContext
    {
        public static DummyCtx Instance = new DummyCtx();

        IDictionary<Guid, Type> typeMap = new Dictionary<Guid, Type>();

        public DummyCtx()
        {
            typeMap[new Guid("D6F50F64-93FA-49b7-93F1-BA66EFAE3E98")] =
            typeMap[new Guid("51654971-0D87-4A51-91E3-A6B53235F3E7")] = typeof(DummyCtx);
            typeMap[new Guid("5D0C5BB6-7DE1-4b1f-A70F-8D1659941941")] = typeof(MakiTimer);
        }

        public event Action OnMain;

        public object GetStaticObject(Guid g)
        {
            return Instance;
        }

        public Type ResolveType(Guid g)
        {
            return typeMap[g];
        }

        internal void DoOnStart()
        {
            OnMain?.Invoke();
        }

        public void HiThere()
        {
            Console.Write("\r" + DateTime.Now + "." + DateTime.Now.Millisecond);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var file = @"E:\testdata\winamp\scripts\timer.maki";
            var vcpu = new VCPU(File.ReadAllBytes(file), DummyCtx.Instance);
            DummyCtx.Instance.DoOnStart();
            Console.Read();
        }
    }
}
