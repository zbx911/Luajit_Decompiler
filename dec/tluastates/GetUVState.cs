namespace Luajit_Decompiler.dec.tluastates
{
    /// <summary>
    /// Gets an upvalue and loads it into a slot.
    /// </summary>
    class GetUVState : BaseState
    {
        public GetUVState(TLuaState state) : base(state)
        {
            state.slots[regs.regA] = state._U[regs.regD];
            new BeginState(state);
        }

        //This state only sets the slot. There is a lua equivalent, but it is unnecessary.
        public override void WriteLua(TLuaState state)
        {
            return;
        }
    }
}
