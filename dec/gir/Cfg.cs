using System;
using System.Collections.Generic;
using Luajit_Decompiler.dec.data;

namespace Luajit_Decompiler.dec.gir
{
    /// <summary>
    /// Control Flow Graph
    /// </summary>
    class Cfg
    {
        private byte[,] adj; //adjacency matrix
        private readonly List<Jump> jumps;
        private List<Block> blocks;

        public Cfg(List<Jump> jumps, List<Block> blocks)
        {
            adj = new byte[blocks.Count, blocks.Count];
            this.jumps = jumps;
            this.blocks = blocks;

            //initialize adj matrix
            for (int i = 0; i < adj.GetLength(0); i++)
                for (int j = 0; j < adj.GetLength(1); j++)
                    adj[i, j] = 0;

            foreach (Jump j in jumps)
            {
                int b1 = FindBlockNameByJIndex(j);
                if (b1 == -2) //-2 is a flag for the very first jump at the top of the file.
                    continue;
                if (b1 == -1)
                    throw new Exception("Cfg:Cfg:: Jump does not exist in any block.");
                int b2 = j.TargetedBlock.GetNameIndex();
                adj[b1, b2] = 1;
            }
            #region debugging adj matrix
            FileManager.ClearDebug();
            for (int i = 0; i < adj.GetLength(0); i++)
                for (int j = 0; j < adj.GetLength(1); j++)
                    if (adj[i, j] == 1)
                        FileManager.WriteDebug("Block[" + i + "] -> Block[" + j + "] :: " + adj[i, j]);
            #endregion
        }

        /// <summary>
        /// Returns a list of indicies of block children.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public List<int> GetChildren(Block b)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < adj.GetLength(1); i++)
                if (adj[b.GetNameIndex(), i] == 1)
                    result.Add(i);
            return result;
        }

        /// <summary>
        /// Returns the index of the parent of a given block.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public int GetParent(Block b)
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
            for (int i = 0; i < blocks.Count; i++)
                if (blocks[i].InstructionExists(j.index) != -1)
                    return blocks[i].GetNameIndex();
            return -1;
        }
    }
}
