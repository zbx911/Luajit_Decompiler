using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.Structures
{
    /// <summary>
    /// Stores jump information including the jump distance, target of jump (what it skips TO. not what it skips over.), and the bci index it was found at.
    /// </summary>
    class Jump
    {
        public int distance; //the distance or target.
        public int index; //where it was found.
        private Block target; //the block it targets. not what it skips over.

        public Jump(BytecodeInstruction jmp, int index)
        {
            if (jmp.opcode != OpCodes.JMP)
                throw new Exception("BCI is not a jump.");
            distance = ((jmp.registers[2] << 8) | jmp.registers[1]) - 0x8000; //can be negative
            this.index = index;
        }

        /// <summary>
        /// TODO: Requires testing.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public Block GetTargetOfJump(Prototype pt, List<Block> blocks)
        {
            if (target == null) //calculate target if it hasn't been calculated already.
            {
                int hunt = index + distance + 1;
                foreach(Block b in blocks)
                {
                    if(b.startB == hunt)
                    {
                        target = b;
                        return b;
                    }
                }
                throw new Exception("Block not found.");
            }
            else return target;
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder("J@" + index + "=>\n");
            res.AppendLine(target.ToString());
            res.AppendLine("end");
            return res.ToString();
        }
    }
}
