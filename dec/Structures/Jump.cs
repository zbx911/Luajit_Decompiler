using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.Structures
{
    class Jump
    {
        public int distance; //the distance or target.
        public int index; //where it was found.

        public Jump(BytecodeInstruction jmp, int index)
        {
            distance = ((jmp.registers[2] << 8) | jmp.registers[1]) - 0x8000; //can be negative
            this.index = index;
        }
    }
}
