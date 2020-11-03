using Luajit_Decompiler.dis;
using System.Runtime.InteropServices;

namespace Luajit_Decompiler.dec.lir
{
    class IntegratedInstruction
    {
        public IRMap iROp;
        public OpCodes originalOp;
        public InstructionRegisters registers;
        public int originalIndex;
        public BytecodeInstruction bci;

        /// <summary>
        /// Contains an IRMap translated opcode and the registers at execution time for a particular bytecode instruction.
        /// </summary>
        /// <param name="iROp"></param>
        /// <param name="originalOp"></param>
        /// <param name="originalIndex">Index of original instruction.</param>
        /// <param name="regA"></param>
        /// <param name="regB"></param>
        /// <param name="regC"></param>
        public IntegratedInstruction(IRMap iROp, BytecodeInstruction bci)
        {
            this.bci = bci;
            this.iROp = iROp;
            registers = bci.regs;
            originalOp = bci.opcode;
            originalIndex = bci.index;
        }

        public override string ToString()
        {
            return " II{ Op: " + iROp.ToString() + ", OriOp: " + originalOp.ToString() + ", A: " + bci.regs.regA + ", C: " + bci.regs.regC + ", B: " + bci.regs.regB + ", D: " + bci.regs.regD +" };";
        }
    }
}
