using Luajit_Decompiler.dis.consts;
using System;
using System.Text;

namespace Luajit_Decompiler.dec.tluastates
{
    class UnaryState : BaseState
    {
        public UnaryState(TLuaState state) : base(state) { }

        //More slot operations...these probably have lua translations associated with them...
        public override void WriteLua(TLuaState state) //write the code first then handle the back end in the constructor...
        {
            switch (state.curII.originalOp)
            {
                case dis.OpCodes.MOV:
                    HandleMov(state);
                    break;
                case dis.OpCodes.NOT:
                    HandleNot(state);
                    break;
                case dis.OpCodes.UNM:
                    HandleUnaryMinus(state);
                    break;
                case dis.OpCodes.LEN:
                    HandleLength(state);
                    break;
            }
        }

        //Copy D into A operation.
        private void HandleMov(TLuaState state)
        {
            #region write
            StringBuilder line = new StringBuilder();
            string dst = state.GetVariableName(state.regs.regA);
            string src = state.GetVariableName(state.regs.regD);
            line.AppendLine("--MOV");

            //if a slot needs to be created, create one and prepend local.
            int check = state.CheckAddSlot(state.slots[state.regs.regD], state.regs.regA);
            if (check == 1)
                line.Append("local ");
            else if (check == -1)
                throw new Exception("UnaryState:HandleMov::Slot failed to add in correct location.");

            line.AppendLine(dst + " = " + src);
            #endregion

            #region op
            state.slots[state.regs.regA] = state.slots[state.regs.regD];
            #endregion
            //debugging
            FileManager.WriteDebug(line.ToString());

            //createSlot = state.regs.regA >= state.slots.Count; //D should already exist if we are moving it into A.
            //if (createSlot) //we need to make a slot for it.
            //{
            //    if (!state.AddSlot(state.slots[state.regs.regD]))
            //        throw new Exception("MOV slot not added in correct location.");
            //    if(state.CheckAddSlot())
            //    line.Append("local ");
            //}
        }

        private void HandleNot(TLuaState state)
        {
            throw new NotImplementedException();
        }

        private void HandleUnaryMinus(TLuaState state)
        {
            throw new NotImplementedException();
        }

        private void HandleLength(TLuaState state)
        {
            throw new NotImplementedException();
        }
    }
}
