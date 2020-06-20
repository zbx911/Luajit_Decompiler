using System;

namespace Luajit_Decompiler.dec.tluastates
{
    /// <summary>
    /// Initializes TLuaState information.
    /// </summary>
    class InitializeState : BaseState
    {
        public InitializeState(TLuaState state) : base(state) { }

        public override void NextState()
        {
            throw new NotImplementedException();
        }

        public override void WriteLua()
        {
            throw new NotImplementedException();
        }
    }
}
