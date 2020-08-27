using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.tluastates
{
    class UnaryState : BaseState
    {
        public UnaryState(TLuaState state) : base(state)
        {
            switch(state.curII.originalOp)
            {
                case dis.OpCodes.MOV:
                    state.slots[regs.regA] = state.slots[regs.regD];
                    break;
                case dis.OpCodes.NOT:
                    break;
                case dis.OpCodes.UNM:
                    break;
                case dis.OpCodes.LEN:
                    break;
            }
            new BeginState(state);
        }

        //TODO: IMPLEMENT METHOD
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

        private void HandleMov(TLuaState state)
        {
            
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
