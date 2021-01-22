using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.lua_formatting;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.state_machine.states.comparisons
{
    class Comparison
    {
        private LStateContext ctx;
        private BytecodeInstruction bci;

        public delegate string DelGetCompRegDValue();

        public Comparison(LStateContext ctx, BytecodeInstruction bci)
        {
            this.ctx = ctx;
            this.bci = bci;
        }

        public string GetCompN()
        {
            return ctx.slots[bci.registers.d].GetValue().ToString();
        }

        public string GetCompP()
        {
            switch (bci.registers.d)
            {
                case 0:
                    return "nil";
                case 1:
                    return "false";
                case 2:
                    return "true";
            }
            return "InvalidComparison";
        }

        public string GetCompS()
        {
            return ctx.string_G[bci.registers.d].GetValue().ToString();
        }

        public string GetCompV()
        {
            return ctx.varNames[bci.registers.d];
        }

        public void HandleLua(DelGetCompRegDValue getRegDValue)
        {
            ConditionalHeader header;

            if (ctx.cfg.IsBlockLoopStart(ctx.currentBlock))
                header = new WhileHeader(bci.opcode,
                    ctx.slots[bci.registers.a].GetValue().ToString(),
                    getRegDValue(),
                    ctx.currentBlock.indent);
            else
                header = new IfHeader(bci.opcode,
                    ctx.slots[bci.registers.a].GetValue().ToString(),
                    getRegDValue(),
                    ctx.currentBlock.indent);

            ctx.lua.AddLuaConstructHeader(header);

            Block trueBlock = ctx.cfg.GetJumpBlockTargetByJIndex(bci.bciIndexInPrototype);
            trueBlock.indent++;
            ctx.blockWriter.blockQueue.Enqueue(trueBlock);

            Block nextBlock = ctx.cfg.GetJumpBlockTargetByJIndex(bci.bciIndexInPrototype + 1);


            if (!ctx.cfg.IsIfStatement(ctx.currentBlock, trueBlock)) //indicitive of an if/else statement.
            {
                ctx.lua.AddElseClause();
                nextBlock.indent++;
                nextBlock.needsEndStatement = true;
            }
            else
                ctx.currentBlock.needsEndStatement = true;

            ctx.blockWriter.blockQueue.Enqueue(nextBlock);
        }
    }
}
