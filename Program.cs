using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//***************\\
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            //d, c, e5 have problems.
            //d fails due to constant section size problems and the off by 1 error.
            //c fails due to lack of table implementation. skip 1 byte over last byte and should work if tables were implemented.
            //e5 fails due to the off by 1 in the final proto header.
            string path = @"C:\Users\Nathan\Desktop\Bytecode\d.lua";
            byte[] bytes = File.ReadAllBytes(path);
            Disassembler dis = new Disassembler(path, bytes);
            dis.Disassemble();
            Console.ReadLine();
        }
    }
}
