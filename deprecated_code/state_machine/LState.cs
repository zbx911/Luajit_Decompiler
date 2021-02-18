using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.lua_formatting;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.state_machine
{
    abstract class LState
    {
        public LStateContext Context { protected get; set; }
        public BytecodeInstruction Bci { protected get; set; }

        public abstract void HandleLua();
        public abstract void HandleSlots();

        //public void CheckScopeAndAddEnds(DecompiledLua lua)
        //{
        //    Block current = Context.cfg.FindBlockByInstructionIndex(Bci.bciIndexInPrototype);
        //    if (current.blockIndex - 1 >= 0)
        //        CheckAddScopeEnds(current);
        //}

        //private void CheckAddScopeEnds(Block current)
        //{
        //    Block previous = Context.cfg.blocks[current.blockIndex - 1];

        //    int scopeDiff = current.scope - previous.scope;
        //    if (scopeDiff >= 0 && Context.cfg.CheckAllRangesIfEndsWithBlockIndex(previous.blockIndex)) //previous must also be the ending block of a block range?
        //        for (int i = 0, curScope = current.scope; i < scopeDiff + 1; i++)
        //            Context.lua.AddScopedEnd(curScope--);
        //}
    }
}
