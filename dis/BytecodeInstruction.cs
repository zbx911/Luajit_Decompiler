using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis
{
    class BytecodeInstruction
    {
        public OpCodes opcode;
        public int[] registers = new int[3]; //these are bytes but decompiler requirements need space of > 255.
        private int registerCount = 0;
        public int index; //index of bci in prototype.

        public BytecodeInstruction(OpCodes op, int index)
        {
            opcode = op;
            this.index = index;
        }

        /// <summary>
        /// Adds registers incrementally. Ex: (OpCode: reg1, reg2, reg3) { OP, A, (BC) or (D) }
        /// </summary>
        /// <param name="b"></param>
        public void AddRegister(int b) //byte. see above.
        {
            registers[registerCount] = b;
            registerCount++;
        }

        /// <summary>
        /// Format: (Opcode): #, #, #;
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "(" + opcode + "): " + registers[0] + ", " + registers[1] + ", " + registers[2] + ";";
        }
    }
}
