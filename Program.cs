using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//***************\\
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dis.Constants;
using Luajit_Decompiler.dec;

namespace Luajit_Decompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Decompiler dec = new Decompiler(new FileManager()); //disassembles and decompiles in the constructor.
        }
    }
}
