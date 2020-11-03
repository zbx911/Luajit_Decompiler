using System.Runtime.InteropServices;

namespace Luajit_Decompiler.dis
{
    [StructLayout(LayoutKind.Explicit)]
    public struct InstructionRegisters
    {
        [FieldOffset(0)]
        public byte regA;

        [FieldOffset(1)]
        public byte regC;

        [FieldOffset(2)] //offset is in bytes.
        public byte regB;

        [FieldOffset(1)]
        public short regD;
    }

    class BytecodeInstruction
    {
        public OpCodes opcode;
        public InstructionRegisters regs;
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
            return "(" + opcode + "): " + "A = " + regs.regA + ", " + "C = " + regs.regC + ", " + "B = " + regs.regB + ", [D: " + regs.regD + "]" +";";
        }
    }
}
