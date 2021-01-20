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

        //TODO: Handle Unsigned shorts as well if necessary.
        //Set A to 16 bit signed integer D.
        private void HandleKShort(TLuaState state)
        {
            //slot operation: slot[A] = num_G[D]
            state.slots[state.regs.regA] = new CShort((short)state.regs.regD);

            //source
            StringBuilder line = new StringBuilder();
            var dst = state.CheckGetVarName(state.regs.regA);
            state.CheckLocal(dst, ref line);
            line.Append(dst.Item2 + " = " + state.regs.regD);
            line.AppendLine(" --KSHORT");
            state.decompLines.Add(line.ToString());

            //debugging
            //FileManager.WriteDebug(line.ToString());
        }

        private void HandleKCData(TLuaState state)
        {
            state.UnimplementedOpcode(state.curII);
        }


        //TODO: troublemaking function...try to figure out if we need to access from slot index or from global table.
        //We also need to handle in-lining any LEN operators as well, but that is handled in LEN function.
        private void HandleKStr(TLuaState state)
        {
            //slot operation: slot[A] = string_G[D]
            state.slots[state.regs.regA] = state.string_G[state.regs.regD];

            //source
            StringBuilder line = new StringBuilder();
            var dst = state.CheckGetVarName(state.regs.regA);
            state.CheckLocal(dst, ref line);
            line.Append(dst.Item2 + " = " + "\"" + state.string_G[state.regs.regD].GetValue() + "\"");
            line.AppendLine(" --KSTR");
            state.decompLines.Add(line.ToString());

            //debugging
            //FileManager.WriteDebug(line.ToString());
        }

        private void HandleKNum(TLuaState state)
        {
            state.UnimplementedOpcode(state.curII);
            //throw new NotImplementedException();
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
            //slot operation: slot[A] = regD->(CBool, CNil)
            if (state.regs.regD == 0)
                state.slots[state.regs.regA] = new CNil();
            else
                state.slots[state.regs.regA] = state.regs.regD == 1 ? new CBool(false) : new CBool(true);

            //source
            StringBuilder line = new StringBuilder();

            var dst = state.CheckGetVarName(state.regs.regA);
            string valueText;

            if (state.regs.regD == 0)
                valueText = "nil";
            else
                valueText = state.regs.regD == 1 ? "false" : "true";

            state.CheckLocal(dst, ref line);
                
            line.Append(dst.Item2 + " = " + valueText);
            line.AppendLine(" --KPRI");
            state.decompLines.Add(line.ToString());

            //debugging
            //FileManager.WriteDebug(line.ToString());
        }

        private void HandleKNil(TLuaState state)
        {
            state.UnimplementedOpcode(state.curII);
        }
    }
}
