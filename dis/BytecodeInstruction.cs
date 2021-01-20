using System.Runtime.InteropServices;

namespace Luajit_Decompiler.dis
{
    [StructLayout(LayoutKind.Explicit)]
    public struct InstructionRegisters
    {
        [FieldOffset(0)]
        public byte a;

        [FieldOffset(1)]
        public byte c;

        [FieldOffset(2)] //offset is in bytes.
        public byte b;

        [FieldOffset(1)]
        public short d;
    }

    class BytecodeInstruction
    {
        public OpCodes opcode;
        public InstructionRegisters registers;
        //public byte regA;
        //public byte regB;
        //public byte regC;
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
            return "(" + opcode + "): " + "A = " + registers.a + ", " + "C = " + registers.c + ", " + "B = " + registers.b + ", [D: " + registers.d + "]" +";";
        }
    }
}
