using System;
using System.Collections.Generic;
using Luajit_Decompiler.dec.Structures;

namespace Luajit_Decompiler.dec.gir
{
    /// <summary>
    /// Control Flow Graph
    /// </summary>
    class Cfg
    {
        private byte[,] adj; //adjacency matrix
        private List<Jump> jumps;
        private List<Block> blocks;
        
        private enum Patterns
        {
            _loop,
            _if,
            _ifelse,
            _ifelseif,
            _ifelseifelse //where elseif can go to N elseifs.
        }

        public Cfg(List<Jump> jumps, List<Block> blocks)
        {
            adj = new byte[blocks.Count, blocks.Count];
            this.jumps = jumps;
            this.blocks = blocks;

            //initialize adj matrix
            for (int i = 0; i < adj.GetLength(0); i++)
                for (int j = 0; j < adj.GetLength(1); j++)
                    adj[i, j] = 0;

            foreach(Jump j in jumps)
            {
                int b1 = FindBlockNameByJIndex(j);
                if (b1 == -2) //-2 is a flag for the very first jump at the top of the file.
                    continue;
                if (b1 == -1)
                    throw new Exception("Cfg:Cfg:: Jump does not exist in any block.");
                int b2 = j.Block.GetNameIndex();
                adj[b1, b2] = 1;
            }
            #region debugging adj matrix
            FileManager.ClearDebug();
            for (int i = 0; i < adj.GetLength(0); i++)
                for (int j = 0; j < adj.GetLength(1); j++)
                    if (adj[i, j] == 1)
                        FileManager.WriteDebug("Block[" + i + "] -> Block[" + j + "] :: " + adj[i,j]);
            #endregion
        }

        public List<Block> GetChildren(Block b)
        {
            throw new NotImplementedException();
        }

        public Block GetParent(Block b)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the name of a block based on the index of a jump. If a jump is not found, return -1.
        /// </summary>
        /// <returns></returns>
        private int FindBlockNameByJIndex(Jump j)
        {
            if (j.index == -1)
                return -2; //returns -2 in the event that the jump is the very first jump that was artifically created at the top of the file.
            for(int i = 0; i < blocks.Count; i++)
                if (blocks[i].InstructionExists(j.index) != -1)
                    return blocks[i].GetNameIndex();
            return -1;
        }
    }
}
