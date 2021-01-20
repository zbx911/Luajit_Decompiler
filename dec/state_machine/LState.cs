using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.state_machine
{
    abstract class LState
    {
        public LStateContext Context { protected get; set; }
        public BytecodeInstruction Bci { protected get; set; }

        public abstract void HandleSlots();
        public abstract void HandleLua();
    }
}
