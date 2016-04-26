using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WhipMaki;
using WhipMaki.Disassembly;

namespace mda
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any()) Fail("No input file specified");
            var file = args.Last();
            if (!File.Exists(file)) Fail(string.Format("The specified file “{0}” doesn't exist", file));
            var bytes = File.ReadAllBytes(args.Last());
            var sda = new ScriptDisassembly(new Maki(bytes));
            sda.Disassemble(null);
            Console.WriteLine(sda.ToString());
        }

        static void Fail(string msg)
        {
            Console.WriteLine(msg);
            Environment.Exit(1);
        }
    }
}
