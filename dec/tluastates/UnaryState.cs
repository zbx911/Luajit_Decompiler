using Luajit_Decompiler.dis.consts;
using System;
using System.Text;

namespace Luajit_Decompiler.dec.tluastates
{
    class UnaryState : BaseState
    {
        private bool createSlot = false;

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

        public override void Operation(TLuaState state)
        {
            switch (state.curII.originalOp)
            {
                case dis.OpCodes.MOV: //TODO: operation MIGHT make a slot...so we need to include that here.
                    state.slots[state.regs.regA] = state.slots[state.regs.regD];
                    break;
                case dis.OpCodes.NOT:
                    break;
                case dis.OpCodes.UNM:
                    break;
                case dis.OpCodes.LEN:
                    break;
            }
        }

        //Copy D into A operation.
        private void HandleMov(TLuaState state)
        {
            StringBuilder line = new StringBuilder();
            string dst = state.GetVariableName(state.regs.regA);
            string src = state.GetVariableName(state.regs.regD);
            line.AppendLine("--MOV");

            //if a slot needs to be created, create one and prepend local.

            createSlot = state.regs.regA >= state.slots.Count; //D should already exist if we are moving it into A.
            if (createSlot) //we need to make a slot for it.
            {
                if (!state.AddSlot(state.slots[state.regs.regD]))
                    throw new Exception("MOV slot not added in correct location.");
                line.Append("local ");
            }

            line.AppendLine(dst + " = " + src);

            //debugging
            FileManager.WriteDebug(line.ToString());
        }

        private void HandleNot(TLuaState state)
        {

        }

        private void HandleUnaryMinus(TLuaState state)
        {

        }

        private void HandleLength(TLuaState state)
        {

        }
    }
}
