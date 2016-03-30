using Microsoft.VisualStudio.TestTools.UnitTesting;
using Whip.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Whip.Scripting.Tests
{
    class DummyCtx : ScriptContext
    {
        public static DummyCtx Instance = new DummyCtx();

        IDictionary<Guid, Type> typeMap = new Dictionary<Guid, Type>();

        public DummyCtx()
        {
            typeMap[new Guid("D6F50F64-93FA-49b7-93F1-BA66EFAE3E98")] = typeof(DummyCtx);
            typeMap[new Guid("51654971-0D87-4a51-91E3-A6B53235F3E7")] = typeof(DummyCtx);
        }

        public event EventHandler<ScriptEventArgs> ScriptEvent;

        public override object GetStaticObject(Guid g)
        {
            return Instance;
        }

        public override Type ResolveType(Guid g)
        {
            return typeMap[g];
        }

        public void TestAssert(bool b)
        {
            Console.WriteLine(b);
        }

        public void Test(int x, int y)
        {
            Console.WriteLine(x);
            Console.WriteLine(y);
        }

        public void Test0()
        {
            Console.WriteLine();
        }

        public void DoOnStart()
        {
            //OnStart?.Invoke();
            OnEvent?.Invoke(3, 24, 78);
        }

        public event Action OnStart;
        public event Action<int, int, int> OnEvent;
    }

    [TestClass()]
    public class VCPUTests
    {
        [TestMethod()]
        public void VCPUTest()
        {
            var file = @"E:\testdata\winamp\scripts\case1.maki";
            var cpu = new VCPU(File.ReadAllBytes(file), DummyCtx.Instance);
            DummyCtx.Instance.DoOnStart();
            //cpu.Run();
            Assert.Fail();
        }
    }
}