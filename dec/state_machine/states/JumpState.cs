namespace Luajit_Decompiler.dec.state_machine.states
{
    class JumpState : LState
    {
        public override void HandleLua()
        {
            if(!Context.cfg.GetParentBlock(Context.currentBlock).visited)
                Context.blockWriter.blockQueue.Enqueue(Context.cfg.GetJumpBlockTargetByJIndex(Bci.index));
        }

        public override void HandleSlots()
        {
            return;
        }
    }
}
