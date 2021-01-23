using Luajit_Decompiler.dec.data;

namespace Luajit_Decompiler.dec.state_machine.states
{
    class JumpState : LState
    {
        public override void HandleLua()
        {
            //Block b = Context.cfg.GetParentBlock(Context.currentBlock);
            Block b = Context.cfg.GetJumpBlockTargetByJIndex(Bci.bciIndexInPrototype);
            if (b != null && !b.visited)
                Context.blockWriter.blockQueue.Enqueue(Context.cfg.GetJumpBlockTargetByJIndex(Bci.bciIndexInPrototype));
        }

        public override void HandleSlots()
        {
            return;
        }
    }
}
