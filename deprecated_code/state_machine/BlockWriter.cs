using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.lua_formatting;
using System.Collections.Generic;

namespace Luajit_Decompiler.dec.state_machine
{
    class BlockWriter
    {
        private Prototype pt;
        private DecompiledLua lua;
        private ControlFlowGraph cfg;

        public LStateContext ctx;

        public BlockWriter(Prototype pt, ControlFlowGraph cfg, DecompiledLua lua)
        {
            this.pt = pt;
            this.lua = lua;
            this.cfg = cfg;
            ctx = new LStateContext(pt, cfg, lua);
        }

        public void WriteBlocks()
        {
            foreach (Block b in cfg.blocks)
                WriteBlock(b);
        }

        public void WriteBlock(Block b)
        {
            //set current block here.
            ctx.currentBlock = b;

            if (b.isElseBlock)
                ctx.lua.WriteSrcElse(b.scope);

            for (int i = 0; i < b.bytecodeInstructions.Length; i++)
                ctx.Transition(b, i);

            if (b.neededEnds > 0)
                ctx.lua.WriteNeededEnds(b);
        }
    }
}
