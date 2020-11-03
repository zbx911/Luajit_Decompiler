namespace Luajit_Decompiler.dec.tluastates.get
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
            //GGET is always a string operand.
            state.slots[state.regs.regA] = state.string_G[state.regs.regD]; //slot A = _G[D]
            state.decompLines.Add("--GGET: " + state.CheckGetVarName(state.regs.regA).Item2 + " = " + state.string_G[state.regs.regD].GetValue() + "\n");
        }
    }
}
