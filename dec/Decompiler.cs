using System;
using System.Collections.Generic;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec
{
    class Decompiler
    {
        public Dictionary<string, List<Prototype>> disFile;
        public FileManager fm;

        public Decompiler(FileManager fm)
        {
            this.fm = fm;
            Disassembler dis = new Disassembler(fm);
            dis.DisassembleAll();
            disFile = dis.DisFile;
            StartDecompilation();
        }

        private void StartDecompilation()
        {
            if (disFile != null)
            {
                foreach (KeyValuePair<string, List<Prototype>> kv in disFile)
                {
                    Decompile(kv.Key, kv.Value);
                    throw new Exception("derp");
                    //fm.WriteDecompiledCode(kv.Key);
                }
            }
            else
                throw new Exception("Disassembled file is null.");
        }

        private void Decompile(string name, List<Prototype> pts)
        {
            //We go backwards here because the 'main' prototype is always the last one and will have the most prototype children.
            //In addition, the 'main' prototype will contain the local variables that are referenced as upvalues in most child prototypes.
            for (int i = pts.Count; i > 0; i--)
            {
                DecompilePrototype dpt = new DecompilePrototype(pts[i - 1]);
                dpt.StartProtoDecomp();
            }
        }
    }
}
