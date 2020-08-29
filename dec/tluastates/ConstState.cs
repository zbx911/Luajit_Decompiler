using Luajit_Decompiler.dis.consts;
using System;
using System.Text;

namespace Luajit_Decompiler.dec.tluastates
{
    class ConstState : BaseState
    {
        public ConstState(TLuaState state) : base(state) { }

        public override void WriteLua(TLuaState state)
        {
            //handle writing variable declaration for a constant.
            switch (state.curII.originalOp)
            {
                case dis.OpCodes.KSHORT:
                    HandleKShort(state);
                    break;
                case dis.OpCodes.KCDATA:
                    HandleKCData(state);
                    break;
                case dis.OpCodes.KSTR:
                    HandleKStr(state);
                    break;
                case dis.OpCodes.KNUM:
                    HandleKNum(state);
                    break;
                case dis.OpCodes.KPRI:
                    HandleKPri(state);
                    break;
                case dis.OpCodes.KNIL:
                    HandleKNil(state);
                    break;
            }
        }

        public override void Operation(TLuaState state)
        {
            //handle slot changes. These are additions to slots.
            switch (state.curII.originalOp)
            {
                case dis.OpCodes.KSHORT:
                    CShort value = new CShort((short)state.regs.regD); //for KSHORT, the short is signed.
                    if (!state.AddSlot(value))
                        throw new Exception("KSHORT slot not added in correct location.");
                    break;

                case dis.OpCodes.KCDATA:
                    break;
                case dis.OpCodes.KSTR:
                    break;
                case dis.OpCodes.KNUM:
                    break;
                case dis.OpCodes.KPRI:
                    break;
                case dis.OpCodes.KNIL:
                    break;
            }
        }

        //Set A to 16 bit signed integer D.
        private void HandleKShort(TLuaState state)
        {
            StringBuilder line = new StringBuilder();
            line.AppendLine("--KSHORT. We assume local variable here for now.");
            string dstName = state.GetVariableName(state.regs.regA);
            line.AppendLine("local " + dstName + " = " + state.regs.regD);
            state.decompLines.Add(line.ToString());

            //debugging
            FileManager.WriteDebug(line.ToString());
        }

        private void HandleKCData(TLuaState state)
        {
            throw new NotImplementedException();
        }

        private void HandleKStr(TLuaState state)
        {
            throw new NotImplementedException();
        }

        private void HandleKNum(TLuaState state)
        {
            throw new NotImplementedException();
        }

        private void HandleKPri(TLuaState state)
        {
            throw new NotImplementedException();
        }

        private void HandleKNil(TLuaState state)
        {
            throw new NotImplementedException();
        }
    }
}
