namespace Luajit_Decompiler.dec.tluastates
{
    /// <summary>
    /// Gets an upvalue and loads it into a slot.
    /// </summary>
    class GetUVState : BaseState
    {
        public GetUVState(TLuaState state) : base(state) { }

        //This state only sets the slot. There is a lua equivalent, but it is unnecessary i think...
        public override void WriteLua(TLuaState state)
        {
            #region op
            state.slots[state.regs.regA] = state._U[state.regs.regD];
            #endregion
            return;
        }
    }
}
