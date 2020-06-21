using Luajit_Decompiler.dis;
using System.Runtime.InteropServices;

namespace Luajit_Decompiler.dec.lir
{
    class IntegratedInstruction
    {
        public IRMap iROp;
        public OpCodes originalOp;
        public Registers registers;
        public int originalIndex;

        /// <summary>
        /// Contains an IRMap translated opcode and the registers at execution time for a particular bytecode instruction.
        /// </summary>
        /// <param name="iROp"></param>
        /// <param name="originalOp"></param>
        /// <param name="originalIndex">Index of original instruction.</param>
        /// <param name="regA"></param>
        /// <param name="regB"></param>
        /// <param name="regC"></param>
        public IntegratedInstruction(IRMap iROp, OpCodes originalOp, int indexOfOp, byte regA, byte regB, byte regC)
        {
            this.iROp = iROp;
            this.originalOp = originalOp;
            this.originalIndex = indexOfOp;
            registers = new Registers();
            registers.regA = regA;
            registers.regB = regB;
            registers.regC = regC;
        }

        public override string ToString()
        {
            return " II{ Op: " + iROp.ToString() + ", OriOp: " + originalOp.ToString() + ", A: " + registers.regA + ", C: " + registers.regC + ", B: " + registers.regB + ", D: " + registers.regD +" };";
        }

        [StructLayout(LayoutKind.Explicit)] //size of byte = 1, size of short = 2, size of int = 4
        public struct Registers
        {
            [FieldOffset(0)]
            public byte regA;

            [FieldOffset(1)]
            public byte regC;

            [FieldOffset(2)]
            public byte regB;

            [FieldOffset(1)] //C union B -> D
            public ushort regD;
        }
    }
}
