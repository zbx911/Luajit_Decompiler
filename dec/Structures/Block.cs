using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis.Constants;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;

namespace Luajit_Decompiler.dec.Structures
{
    class Block
    {
        public int startB; //start of the block. (Relative to the asm lines).
        public int endB; //end of the block.
        public List<BytecodeInstruction> bcis; //all the bytecode instructions in a block.
        public bool written; //has the block already been written to source?
        public int iCount { get { return bcis.Count; } } //instruction count.
        public int nextJumpIndex { get { return endB + 1; } } //location of the jump after this block. can be null in event of RET0 causing index out of bounds error. (bci.count = ret0 location).

        public Block()
        {
            written = false;
        }
    }
}
