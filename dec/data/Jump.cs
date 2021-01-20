using System.Text;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.data
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
                distance = ((jmp.registers.b << 8) | jmp.registers.c) - 0x8000;
            else if (jumpType == 3)
                distance = 1; //conditionals/returns
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
