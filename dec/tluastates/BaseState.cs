using static Luajit_Decompiler.dec.lir.IntegratedInstruction;

namespace Luajit_Decompiler.dec.tluastates
{
    abstract class BaseState
    {
        protected Registers regs; //registers from the current integrated instruction.

        //Child constructors act as sort of the things to do in the background AFTER the lua has been written such as setting slots.
        public BaseState(TLuaState state)
        {
            regs = state.curII.registers;
            WriteLua(state);
        }
        public abstract void WriteLua(TLuaState state); //THIS IS CALLED FIRST BEFORE ANY CHILD CONSTRUCTOR IS CALLED.
    }
}
