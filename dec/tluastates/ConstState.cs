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

        //Set A to 16 bit signed integer D.
        private void HandleKShort(TLuaState state)
        {
            #region write
            StringBuilder line = new StringBuilder();
            line.AppendLine("--KSHORT. We assume local variable here for now.");
            string dstName = state.GetVariableName(state.regs.regA);
            line.AppendLine("local " + dstName + " = " + state.regs.regD);
            state.decompLines.Add(line.ToString());
            #endregion

            #region op
            CShort value = new CShort((short)state.regs.regD); //for KSHORT, the short is signed.
            state.CheckAddSlot(value, state.regs.regA);
            #endregion

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

        /// <summary>
        /// Sets register A to one of the following from register D: nil, true, false.
        /// Note: A single nil value is set with KPRI. KNIL is only used when multiple values need to be set to nil.
        /// Potential register D values:
        ///     nil = 0
        ///     false = 1
        ///     true = 2
        /// </summary>
        /// <param name="state"></param>
        private void HandleKPri(TLuaState state)
        {
            #region write
            StringBuilder line = new StringBuilder();
            line.AppendLine("--KPRI");
            string dst = state.GetVariableName(state.regs.regA);
            BaseConstant value;
            string valueText;
            if (state.regs.regD == 0)
            {
                value = new CNil();
                valueText = "nil";
            }
            else
            {
                bool v = state.regs.regD == 1 ? false : true;
                valueText = v.ToString().ToLower();
                value = new CBool(v);
            }
            int check = state.CheckAddSlot(value, state.regs.regA);
            if (check == 1)
                line.Append("local ");
            line.AppendLine(dst + " = " + valueText);
            state.decompLines.Add(line.ToString());
            #endregion

            #region op
            state.slots[state.regs.regA] = value; //set slot at A to value corresponding to regD.
            #endregion

            //debugging
            FileManager.WriteDebug(line.ToString());
        }

        private void HandleKNil(TLuaState state)
        {
            throw new NotImplementedException();
        }
    }
}
