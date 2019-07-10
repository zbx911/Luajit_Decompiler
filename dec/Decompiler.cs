using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            disFile = dis.disFile;
            Decompile();
        }

        /// <summary>
        /// Decompile each file and its prototypes.
        /// </summary>
        private void Decompile()
        {
            if (disFile != null)
            {
                //for each file, decompile their prototypes.
                foreach (KeyValuePair<string, List<Prototype>> kv in disFile)
                {
                    DecPrototypes dp = new DecPrototypes(kv.Key, kv.Value);
                    fm.WriteDecompiledCode(kv.Key, dp.ToString());
                }
            }
            else
                throw new Exception("Disassembled file is null.");
        }
    }
}
