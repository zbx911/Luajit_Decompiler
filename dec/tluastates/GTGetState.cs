namespace Luajit_Decompiler.dec.tluastates
{
    /// <summary>
    /// Gets from the global constants table and loads it into a slot.
    /// </summary>
    class GTGetState : BaseState
    {
        public GTGetState(TLuaState state) : base(state) { }

        //This state only sets the slot. There is a lua equivalent...it may be necessary.
        public override void WriteLua(TLuaState state)
        {
            #region op
            state.slots[state.regs.regA] = state._G[state.regs.regD]; //slot A = _G[D]
            #endregion
            return;
        }
    }
}
