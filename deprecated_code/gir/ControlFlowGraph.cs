using System;
using System.Collections.Generic;
using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.lir;

namespace Luajit_Decompiler.dec.gir
{
    /// <summary>
    /// Control Flow Graph
    /// </summary>
    class ControlFlowGraph
    {
        private byte[,] adj; //adjacency matrix where adj[i,j] = Child of I called J has an indentation/nest level of adj[i,j] - 1.
        public readonly List<Jump> jumps;
        public readonly List<Block> blocks; //index in this list and block name should be the same.

        public ControlFlowGraph(ref List<Jump> jumps, ref List<Block> blocks)
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
                int b1 = FindBlockIndexByInstructionIndex(j.index);
                if (b1 == -2) //-2 is a flag for the very first jump at the top of the file.
                    continue;
                if (b1 == -1)
                    throw new Exception("Cfg:Cfg:: Jump does not exist in any block.");
                int b2 = j.TargetedBlock.GetNameIndex();
                adj[b1, b2] = 1;
            }

            //increment each skipped over block in the adj matrix.
            //TODO: Might want to consider trying to make this more efficient. It is a little slow with the number of nested loops. (3) -> O(N^3)?
            for(int i = 1; i < jumps.Count; i++) //skip the first jump which is -1
            {
                int[] skipped = JumpSkipsOver(jumps[i]);
                if(skipped.Length > 0)
                {
                    for(int j = 0; j < skipped.Length; j++)
                    {
                        int parent = GetParent(blocks[skipped[j]]); //get the parent which is i in [i,j]
                        adj[parent, skipped[j]]++; //increment
                    }
                }
            }


            #region debugging adj matrix
            FileManager.ClearDebug();
            for (int i = 0; i < adj.GetLength(0); i++)
                for (int j = 0; j < adj.GetLength(1); j++)
                    if (adj[i, j] >= 1)
                        FileManager.WriteDebug("iBlock[" + i + "] -> jBlock[" + j + "] :: " +
                            "Indentation of Parent Block: " + GetIndentationLevel(blocks[i]) + ";" + 
                            " Indentation of Child Block: " + (adj[i, j] - 1) + ";");
            #endregion
        }

        //Checks if a conditional integrated instruction is the start of a loop or an if statement.
        public bool IIStartsLoop(IntegratedInstruction ii)
        {
            //look at the next block, see if it points back to us.
            return false;
        }

        /// <summary>
        /// Returns block indices which the jump has skipped over. Can return an empty array.
        /// </summary>
        /// <param name="j"></param>
        /// <returns></returns>
        private int[] JumpSkipsOver(Jump j)
        {
            List<int> result = new List<int>();

            int origin = FindBlockIndexByInstructionIndex(j.index); //origin block
            int targeted = j.TargetedBlock.GetNameIndex();

            while (++origin < targeted)
                result.Add(origin);

            return result.ToArray();
        }

        /// <summary>
        /// Returns a list of indicies of block children.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public int[] GetChildren(Block b)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < adj.GetLength(1); i++)
                if (adj[b.GetNameIndex(), i] == 1)
                    result.Add(i);
            return result.ToArray();
        }

        /// <summary>
        /// Returns the index of the parent of a given block. -1 if not found.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public int GetParent(Block b)
        {
            for (int i = 0; i < adj.GetLength(0); i++)
                if (adj[i, b.GetNameIndex()] >= 1)
                    return i;
            return -1;
        }

        /// <summary>
        /// Returns the indentation level of a given block.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public int GetIndentationLevel(Block b)
        {
            int pIndex = GetParent(b);
            if (pIndex == -1) return 0;
            return adj[pIndex, b.GetNameIndex()] - 1;
        }

        /// <summary>
        /// Returns the name of a block based on the index of a jump. If a jump is not found, return -1.
        /// </summary>
        /// <returns></returns>
        private int FindBlockIndexByInstructionIndex(int index)
        {
            if (index == -1)
                return -2; //returns -2 in the event that the jump is the very first jump that was artifically created at the top of the file.
            for (int i = 0; i < blocks.Count; i++)
                if (blocks[i].InstructionExists(index) != -1)
                    return blocks[i].GetNameIndex();
            return -1;
        }

        private Block FindBlockByInstructionIndex(int index)
        {
            return blocks[FindBlockIndexByInstructionIndex(index)];
        }
    }
}
