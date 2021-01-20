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
        public Queue<Block> blockQueue;

        public BlockWriter(Prototype pt, ControlFlowGraph cfg, DecompiledLua lua)
        {
            this.pt = pt;
            this.lua = lua;
            this.cfg = cfg;
            ctx = new LStateContext(pt, cfg, lua, this);
            blockQueue = new Queue<Block>();
        }

        public void WriteBlocks()
        {
            blockQueue.Enqueue(cfg.blocks[0]);
            while(blockQueue.Count > 0)
                WriteBlock(blockQueue.Dequeue(), ctx);
        }

        public void WriteBlock(Block b, LStateContext ctx)
        {
            if (b.visited) return;
            ctx.currentBlock = b;

            for (int i = 0; i < b.bytecodeInstructions.Length; i++)
                ctx.Transition(StateMap.GetState(b.bytecodeInstructions[i].opcode), b.bytecodeInstructions[i]);

            if (b.needsEndStatement)
            {
                if(ctx.lua.indent > 0)
                    ctx.lua.indent--;
                lua.AddEnd();
            }

            b.visited = true;
        }
    }
}
