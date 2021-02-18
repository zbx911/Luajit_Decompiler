using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.state_machine.states
{
    class ConstStateRef : LState
    {
        public override void HandleLua()
        {

        }

        public override void HandleSlots()
        {
            Context.slots[Bci.registers.a] = Bci.opcode == OpCodes.KSTR ? Context.string_G[Bci.registers.d] : Context.num_G[Bci.registers.d];
        }
    }
}
