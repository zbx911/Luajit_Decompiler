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
            string path = @"C:\Users\Nathan\Desktop\Bytecode\f3.lua";
            byte[] bytes = File.ReadAllBytes(path);
            Disassembler dis = new Disassembler(path, bytes);
            dis.Disassemble();
            Console.ReadLine();
        }
    }
}
