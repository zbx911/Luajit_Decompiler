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
            FileManager fm = new FileManager();
            Disassembler dis = new Disassembler(fm);
            dis.DisassembleAll();
        }
    }
}
