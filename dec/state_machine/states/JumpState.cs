namespace Luajit_Decompiler.dec.state_machine.states
{
    class JumpState : LState
    {
        public override void HandleLua()
        {
            if(!Context.cfg.GetParentBlock(Context.currentBlock).visited) //add null check.
                Context.blockWriter.blockQueue.Enqueue(Context.cfg.GetJumpBlockTargetByJIndex(Bci.bciIndexInPrototype));
        }

        public override void HandleSlots()
        {
            return;
        }
    }
}
