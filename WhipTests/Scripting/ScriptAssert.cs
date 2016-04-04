using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhipTests.Scripting
{
    class ScriptAssert
    {
        public void IsTrue(bool b) => Assert.IsTrue(b);
        public event Action OnMain;

        public void Run()
        {
            OnMain?.Invoke();
        }
    }
}
