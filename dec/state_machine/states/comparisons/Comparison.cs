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
            return ctx.num_G[bci.registers.d].GetValue().ToString();
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
            return ctx.varNames.GetVariableName(bci.registers.d);
        }

        public void HandleLua(DelGetCompRegDValue getRegDValue)
        {
            ctx.lua.AddLuaConstructHeader(GetComparisonHeader(getRegDValue));

            Block trueBlock = ctx.cfg.GetJumpBlockTargetByJIndex(bci.bciIndexInPrototype);
            ctx.blockWriter.WriteIndentedBlock(trueBlock, ctx);

            Block nextBlock = ctx.cfg.GetJumpBlockTargetByJIndex(bci.bciIndexInPrototype + 1);
            if (!ctx.cfg.IsIfStatement(ctx.cfg.FindBlockByInstructionIndex(bci.bciIndexInPrototype), trueBlock)) //indicitive of an if/else statement.
            {
                ctx.lua.AddElseClause();
                ctx.blockWriter.WriteIndentedBlock(nextBlock, ctx);
            }
            ctx.lua.AddEnd();
        }

        private ConditionalHeader GetComparisonHeader(DelGetCompRegDValue getRegDValue)
        {
            ConditionalHeader header;

            if (ctx.cfg.IsBlockLoopStart(ctx.cfg.FindBlockByInstructionIndex(bci.bciIndexInPrototype)))
                header = new WhileHeader(bci.opcode,
                    ctx.varNames.GetVariableName(bci.registers.a),
                    getRegDValue(),
                    ctx.lua.indent);
            else
                header = new IfHeader(bci.opcode,
                    ctx.varNames.GetVariableName(bci.registers.a),
                    getRegDValue(),
                    ctx.lua.indent);

            return header;
        }
    }
}
