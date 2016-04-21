using System.IO;
using WhipMaki;

namespace Whip.Scripting.Tests
{
    internal class TestUtil
    {
        public static void RunScript(string file)
        {
            var ctx = DummyCtx.Instance;
            var vcpu = new VCPU(File.ReadAllBytes(file), ctx);
            ctx.DoOnStart();
            vcpu.Unsubscribe();
        }
    }
}