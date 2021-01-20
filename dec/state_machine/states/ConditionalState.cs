using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.lua_formatting;

namespace Luajit_Decompiler.dec.state_machine.states
{
    class ConditionalState : LState
    {
        public override void HandleLua()
        {
            ConditionalHeader header; //TODO: REFACTOR: pass Bci, Context. Test the op to see which array we are accessing.

            if (Context.cfg.IsBlockLoopStart(Context.currentBlock))
                header = new WhileHeader(Bci.opcode, 
                    Context.slots[Bci.registers.a].GetValue().ToString(),
                    Context.slots[Bci.registers.d].GetValue().ToString(),
                    Context.currentBlock.indent);
            else
                header = new IfHeader(Bci.opcode,
                    Context.slots[Bci.registers.a].GetValue().ToString(),
                    Context.slots[Bci.registers.d].GetValue().ToString(),
                    Context.currentBlock.indent);

            Context.lua.AddLuaConstructHeader(header);

            Block trueBlock = Context.cfg.GetJumpBlockTargetByJIndex(Bci.index);
            trueBlock.indent++;
            Context.blockWriter.blockQueue.Enqueue(trueBlock);

            Block nextBlock = Context.cfg.GetJumpBlockTargetByJIndex(Bci.index + 1);


            if (!Context.cfg.IsIfStatement(Context.currentBlock, trueBlock)) //indicitive of an if/else statement.
            {
                Context.lua.AddElseClause();
                nextBlock.indent++;
                nextBlock.needsEndStatement = true;
            }
            else
                Context.currentBlock.needsEndStatement = true;
            
            Context.blockWriter.blockQueue.Enqueue(nextBlock);

            //Context.blockWriter.WriteBlock(Context.cfg.GetJumpBlockTargetByJIndex(Bci.index), Context);
            //BlockWriter.WriteBlock(Context.cfg.blocks[Context.currentBlock.blockIndex + 1], Context); //recursive call. WriteBlock also called the current state.
            //Context.indent--;
            //Context.lua.AddEnd();
        }

        public override void HandleSlots()
        {
            return;
        }
    }
}
