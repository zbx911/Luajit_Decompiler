using static Luajit_Decompiler.dec.lir.IntegratedInstruction;

namespace Luajit_Decompiler.dec.tluastates
{
    abstract class BaseState
    {
        protected Registers regs; //registers from the current integrated instruction.

        public BaseState(TLuaState state)
        {
            regs = state.curII.registers;
            WriteLua(state);
        }
        public abstract void WriteLua(TLuaState state); //THIS IS CALLED FIRST BEFORE ANY CHILD CONSTRUCTOR IS CALLED.
    }
}
