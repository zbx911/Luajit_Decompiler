using static Luajit_Decompiler.dec.lir.IntegratedInstruction;

namespace Luajit_Decompiler.dec.tluastates
{
    abstract class BaseState
    {
        protected bool backToBeginState = true; //whether or not child classes return to BeginState.

        //Child constructors act as sort of the things to do in the background AFTER the lua has been written such as setting slots.
        public BaseState(TLuaState state)
        {
            WriteLua(state);
            if (backToBeginState)
                new BeginState(state);
        }
        public abstract void WriteLua(TLuaState state); //write the necessary lua. Runs first.
    }
}
