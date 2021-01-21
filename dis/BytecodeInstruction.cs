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

        [FieldOffset(2)]
        public byte b;

        [FieldOffset(1)]
        public short d;
    }

    /// <summary>
    /// Container for bytecode instructions within a prototype.
    /// </summary>
    class BytecodeInstruction
    {
        public OpCodes opcode;
        public InstructionRegisters registers;
        public int bciIndexInPrototype;

        public BytecodeInstruction(OpCodes op, int index)
        {
            opcode = op;
            bciIndexInPrototype = index;
        }

        /// <summary>
        /// Format: (Opcode): #, #, #, [#];
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "(" + opcode + "): " + "A = " + registers.a + ", " + "C = " + registers.c + ", " + "B = " + registers.b + ", [D: " + registers.d + "]" +";";
        }
    }
}
