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
        public int nameIndex; //used for labeling blocks.
        public List<BytecodeInstruction> bcis; //all the bytecode instructions in a block.
        public string label;

        public Block(int sIndex, int nameIndex)
        {
            this.sIndex = sIndex;
            this.nameIndex = nameIndex;
            label = "Block[" + nameIndex + "]";
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.AppendLine(label);
            foreach (BytecodeInstruction bci in bcis)
                res.AppendLine(bci.ToString());
            res.AppendLine(label + " end");
            return res.ToString();
        }
    }
}
