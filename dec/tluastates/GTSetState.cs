namespace Luajit_Decompiler.dec.tluastates
{
    /// <summary>
    /// Sets a global table slot to a A. 
    /// TODO: TEST METHOD AGAINST BYTECODE
    /// I don't know if it is referring to the value at the global constant or overwriting the constant itself...we will have to test that out later.
    /// </summary>
    class GTSetState : BaseState
    {
        public GTSetState(TLuaState state) : base(state)
        {
            state._G[regs.regD] = state.slots[regs.regA]; //_G[D] = A
            new BeginState(state);
        }

        //only sets the slot. no lua writing needed.
        public override void WriteLua(TLuaState state)
        {
            return;
        }
    }
}
