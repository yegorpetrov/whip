using Microsoft.VisualStudio.TestTools.UnitTesting;
using Whip.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WhipTests.Scripting;

namespace Whip.Scripting.Tests
{
    class DummyCtx : ScriptContext
    {
        public static DummyCtx Instance = new DummyCtx();

        IDictionary<Guid, Type> typeMap = new Dictionary<Guid, Type>();

        public DummyCtx()
        {
            typeMap[new Guid("F641B031-80A9-4C90-8104-B635981F29E4")] =
            typeMap[new Guid("51654971-0D87-4A51-91E3-A6B53235F3E7")] = typeof(DummyCtx);
            typeMap[new Guid("340E4D7D-B394-4B58-90EB-14523F93D3C5")] = typeof(object);
        }

        public void IsTrue(bool b) => Assert.IsTrue(b);
        public event Action Main;

        public override object GetStaticObject(Guid g)
        {
            return Instance;
        }

        public override Type ResolveType(Guid g)
        {
            return typeMap[g];
        }

        internal void DoOnStart()
        {
            Main?.Invoke();
        }
    }
}