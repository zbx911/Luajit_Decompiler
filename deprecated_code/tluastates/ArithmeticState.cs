using System;
using System.Text;
using Luajit_Decompiler.dec.lir;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.tluastates
{
    //TODO: Implement in-line expressions at a later date.
    class ArithmeticState : BaseState
    {
        public ArithmeticState(TLuaState state) : base(state)
        {
        }

        public override void WriteLua(TLuaState state)
        {
            DelOperandOrder OperandOrder = null;
            switch (state.curII.iROp)
            {
                case IRMap.MathNV:
                    OperandOrder = HandleNV;
                    break;
                case IRMap.MathVN:
                    OperandOrder = HandleVN;
                    break;
                case IRMap.MathVV:
                    OperandOrder = HandleVV;
                    break;
            }
            StringBuilder line = new StringBuilder();
            var dst = state.CheckGetVarName(state.regs.regA);
            state.CheckLocal(dst, ref line);
                
            line.Append(dst.Item2 + " = ");

            //operator...might want to redesign this into the IRmap or something?
            string op = "!@#$%"; //junk value to begin with in case of error.
            switch(state.curII.originalOp)
            {
                case OpCodes.ADDNV:
                case OpCodes.ADDVN:
                case OpCodes.ADDVV:
                    op = " + ";
                    break;
                case OpCodes.SUBNV:
                case OpCodes.SUBVN:
                case OpCodes.SUBVV:
                    op = " - ";
                    break;
                case OpCodes.MULNV:
                case OpCodes.MULVN:
                case OpCodes.MULVV:
                    op = " * ";
                    break;
                case OpCodes.DIVNV:
                case OpCodes.DIVVN:
                case OpCodes.DIVVV:
                    op = " / ";
                    break;
                case OpCodes.MODNV:
                case OpCodes.MODVN:
                case OpCodes.MODVV:
                    op = " % ";
                    break;
                case OpCodes.POW:
                    op = " ^ ";
                    break;
            }

            line.Append(OperandOrder(state, dst, op));
            line.AppendLine(" --ARITHMETIC");
            state.decompLines.Add(line.ToString());
        }

        private delegate string DelOperandOrder(TLuaState state, Tuple<bool, string> checkGetVarNameDst, string op);

        private string HandleVV(TLuaState state, Tuple<bool, string> dst, string op)
        {
            var opC = state.CheckGetVarName(state.regs.regC);
            var opB = state.CheckGetVarName(state.regs.regB);
            return opC.Item2 + op + opB.Item2;
        }
        private string HandleVN(TLuaState state, Tuple<bool, string> dst, string op)
        {
            return state.CheckGetVarName(state.regs.regB).Item2 + op + state.num_G[state.regs.regC].GetValue();
        }
        private string HandleNV(TLuaState state, Tuple<bool, string> dst, string op)
        {
            return state.num_G[state.regs.regC].GetValue() + op + state.CheckGetVarName(state.regs.regB).Item2;
        }
    }
}
