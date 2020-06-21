﻿namespace Luajit_Decompiler.dec.tluastates
{
    /// <summary>
    /// Gets from the global constants table and loads it into a slot.
    /// </summary>
    class GTGetState : BaseState
    {
        public GTGetState(TLuaState state) : base(state)
        {
            state.slots[regs.regA] = state._G[regs.regD]; //slot A = _G[D]
            new BeginState(state);
        }

        //This state only sets the slot. There is a lua equivalent, but it is unnecessary.
        public override void WriteLua(TLuaState state)
        {
            return;
        }
    }
}
