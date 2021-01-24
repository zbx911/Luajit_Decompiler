using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.lua_formatting;
using System.Collections.Generic;
using System;

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
            int i = 0;
            blockQueue.Enqueue(cfg.blocks[0]);

            while (blockQueue.Count > 0)
            {
                WriteBlock(blockQueue.Dequeue(), ctx);
                i++;
                if(blockQueue.Count == 0 && i < cfg.blocks.Count)
                    blockQueue.Enqueue(cfg.blocks[i]);
            }
        }

        public void WriteBlock(Block b, LStateContext ctx)
        {
            if (b.visited) return;

            for (int i = 0; i < b.bytecodeInstructions.Length; i++)
                ctx.Transition(StateMap.GetState(b.bytecodeInstructions[i].opcode), b.bytecodeInstructions[i]);
            b.visited = true;
        }

        public void WriteIndentedBlock(Block b, LStateContext ctx)
        {
            lua.indent++;
            WriteBlock(b, ctx);
            lua.indent--;
        }
    }
}
