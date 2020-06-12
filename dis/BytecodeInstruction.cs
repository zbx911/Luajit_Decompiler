namespace Luajit_Decompiler.dis
{
    class BytecodeInstruction
    {
        public OpCodes opcode;
        public byte regA;
        public byte regB;
        public byte regC;
        public int index; //index of bci in prototype.

        public BytecodeInstruction(OpCodes op, int index)
        {
            opcode = op;
            this.index = index;
        }

        /// <summary>
        /// Format: (Opcode): #, #, #;
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "(" + opcode + "): " + "A = " + regA + ", " + "C = " + regC + ", " + "B = " + regB + ";";
        }
    }
}
