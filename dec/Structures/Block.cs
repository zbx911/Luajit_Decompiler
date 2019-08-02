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
        public int startB; //start of the block. (Relative to the asm lines).
        public int endB; //end of the block.
        private List<BytecodeInstruction> bcis; //all the bytecode instructions in a block.
        public int iCount { get { return bcis.Count; } } //instruction count.
        public string label;
        private Prototype pt;

        public Block(Prototype pt)
        {
            this.pt = pt;
            //determine BCIs by using startB and endB.
        }

        public List<BytecodeInstruction> GetBcis()
        {
            label = "b" + startB + ":" + (endB - 1) + " " + "j@" + (startB - 1); //Note: this may not be accurate for negative jumps. (j@)
            if (bcis == null)
            {
                bcis = new List<BytecodeInstruction>();
                for (int i = startB; i < endB; i++)
                    bcis.Add(pt.bytecodeInstructions[i]);
                return bcis;
            }
            else
                return bcis;

        }

        public override string ToString()
        {
            if (bcis == null)
                GetBcis();
            StringBuilder res = new StringBuilder("---Block---\r\n");
            foreach (BytecodeInstruction bci in bcis)
                res.AppendLine(bci.ToString());
            res.AppendLine("---End Block---");
            return res.ToString();
        }
    }
}
