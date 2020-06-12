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
