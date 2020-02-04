using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.lir
{
    class IRParent
    {
        public IRIMap.IRMap iROp;
        public OpCodes originalOp;
        public uint regA;
        public uint regB;
        public uint regC;

        public IRParent(IRIMap.IRMap iROp, OpCodes originalOp, uint regA, uint regB, uint regC)
        {
            this.iROp = iROp;
            this.originalOp = originalOp;
            this.regA = regA;
            this.regB = regB;
            this.regC = regC;
        }

        protected virtual uint GetRegD()
        {
            return uint.MaxValue;
        }
    }
}
