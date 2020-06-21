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
        public override void WriteLua(TLuaState state)
        {
            throw new System.NotImplementedException();
        }
    }
}
