using Luajit_Decompiler.dec.lir;

namespace Luajit_Decompiler.dec.tluastates
{
    /// <summary>
    /// The state that is returned to to check the integrated instruction (II) and point it to the correct state for writing lua source.
    /// </summary>
    class BeginState : BaseState
    {
        public BeginState(TLuaState state) : base(state) { }

        //We do not write any lua in this state.
        public override void WriteLua(TLuaState state)
        {
            return;
        }

        public override void Operation(TLuaState state)
        {
            backToBeginState = false;
            state.NextII();
            switch (state.curII.iROp)
            {
                case IRMap.GTGet: //global table get.
                    new GTGetState(state);
                    break;
                case IRMap.GetUV: //get upvalue from upvalue table.
                    new GetUVState(state);
                    break;
                case IRMap.GTSet:
                    new GTSetState(state);
                    break;
                case IRMap.Unary:
                    new UnaryState(state);
                    break;
                case IRMap.Const:
                    new ConstState(state);
                    break;
                #region Skip over these instructions
                case IRMap.Goto: //we do not worry about jumps anymore since Cfg has the control flow recorded.
                default:
                    new BeginState(state); //skip over...
                    break;
                    #endregion
            }
            return;
        }
    }
}
