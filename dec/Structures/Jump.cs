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
        public int jumpType; //1 = jmp, 2 = ret, 3 = comparison
        public int target;

        public Block TargetedBlock { get; set; }

        public Jump(BytecodeInstruction jmp, int jumpType, int nameIndex, Prototype pt)
        {
            index = jmp.index;
            this.jumpType = jumpType;
            if (jumpType == 1) //calculate jump distance. may be negative.
                distance = ((jmp.registers[2] << 8) | jmp.registers[1]) - 0x8000;
            else if (jumpType == 3)
                distance = 1; //conditionals/returns
            //target = new Block(index + distance + 1, nameIndex, pt); //+1 to avoid off by 1 error for start of a block.
            target = index + distance + 1; //the target bci.
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder("J@" + index + "=>\n");
            res.AppendLine(target.ToString());
            return res.ToString();
        }
    }
}
