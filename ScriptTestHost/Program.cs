using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Whip.Scripting;

namespace ScriptTestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = @"E:\testdata\winamp\Big Bento\scripts\infoline.maki";
            var vcpu = new VCPU(File.ReadAllBytes(file), null);
        }
    }
}
