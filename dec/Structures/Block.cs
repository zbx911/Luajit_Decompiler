using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis.Constants;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;
using Luajit_Decompiler.dec;

namespace Luajit_Decompiler.dec.Structures
{
    class Block
    {
        public int sIndex; //start of the block. (Relative to the asm lines).
        public int eIndex; //end of block.
        public int nameIndex; //used for labeling blocks.
        public List<BytecodeInstruction> bcis; //all the bytecode instructions in a block.
        public string label;
        private bool finalized = false; //for error checking.

        public Block(int sIndex, int nameIndex) //TODO: Find way to fill in bcis array. Though it probably isn't necessary.
        {
            this.sIndex = sIndex;
            this.nameIndex = nameIndex;
            label = "Block[" + nameIndex + "]";
        }

        public void Finalize(int eIndex)
        {
            this.eIndex = eIndex;
            bcis = new List<BytecodeInstruction>();
            for (int i = sIndex; i < eIndex; i++)
                bcis.Add(DecPrototypes.pt.bytecodeInstructions[i]);
            finalized = true;
        }

        public override string ToString()
        {
            if (!finalized)
                throw new Exception("Error. Block not finalized.");
            StringBuilder res = new StringBuilder();
            res.AppendLine(label);
            foreach (BytecodeInstruction bci in bcis)
                res.AppendLine(bci.index + ":" + bci.ToString());
            return res.ToString();
        }
    }
}
