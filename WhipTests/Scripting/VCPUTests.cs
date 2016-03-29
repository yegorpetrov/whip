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
    class DummyCtx : ScriptContext, IScriptable
    {
        IDictionary<Guid, Type> typeMap = new Dictionary<Guid, Type>();

        public DummyCtx()
        {
            typeMap[new Guid("D6F50F64-93FA-49b7-93F1-BA66EFAE3E98")] = typeof(DummyCtx);
        }

        public event EventHandler<ScriptEventArgs> ScriptEvent;

        public override object GetStaticObject(Guid g)
        {
            return Activator.CreateInstance(typeMap[g]);
        }

        public object ScriptMethod(string name, object[] args)
        {
            switch (name)
            {
                case "Assert":
                    Assert.IsTrue(Convert.ToBoolean(args[0]));
                    break;
            }
            return null;
        }
    }

    [TestClass()]
    public class VCPUTests
    {
        [TestMethod()]
        public void VCPUTest()
        {
            var ctx = new DummyCtx();
            var file = @"E:\testdata\winamp\scripts\case1.maki";
            var cpu = new VCPU(File.ReadAllBytes(file), ctx);
            cpu.Run();
            Assert.Fail();
        }
    }
}