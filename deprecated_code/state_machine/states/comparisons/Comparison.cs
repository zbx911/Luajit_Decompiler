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
            ctx.lua.WriteSrcConstruct(GetComparisonHeader(getRegDValue), ctx.currentBlock.scope);
            ScopifyRangeAndMarkEnd();
            CheckAndMarkElseBlock();
        }

        private void ScopifyRangeAndMarkEnd()
        {
            Block jumpTarget = ctx.cfg.GetJumpBlockTargetByJIndex(bci.bciIndexInPrototype + 1);
            BlockRange br = new BlockRange(ctx.cfg, ctx.currentBlock.blockIndex, jumpTarget.blockIndex);
            br.IncrementBlockRangeScope();
            if(br.Range != null)
                br.Range[br.Range.Length - 1].neededEnds++;
        }

        private void CheckAndMarkElseBlock()
        {
            Block selfTarget = ctx.cfg.GetJumpBlockTargetByJIndex(bci.bciIndexInPrototype);
            Block[] children = ctx.cfg.GetChildBlocks(selfTarget);

            if (children[0] != null && selfTarget.blockIndex + 1 < children[0].blockIndex + 1) //basically if comparison block's true block does NOT point to trueblock+1. AKA an else statement?
                selfTarget.isElseBlock = true;
        }

        private ConditionalHeader GetComparisonHeader(DelGetCompRegDValue getRegDValue)
        {
            ConditionalHeader header;

            if (ctx.cfg.IsBlockLoopStart(ctx.cfg.FindBlockByInstructionIndex(bci.bciIndexInPrototype)))
                header = new WhileHeader(bci.opcode,
                    ctx.varNames.GetVariableName(bci.registers.a),
                    getRegDValue(),
                    ctx.currentBlock.scope);
            else
                header = new IfHeader(bci.opcode,
                    ctx.varNames.GetVariableName(bci.registers.a),
                    getRegDValue(),
                    ctx.currentBlock.scope);

            return header;
        }
    }
}
