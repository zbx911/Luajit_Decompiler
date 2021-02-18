using System;
using System.Collections.Generic;

namespace Luajit_Decompiler.dec.data
{
    class ControlFlowGraph
    {
        private byte[,] adj;
        public readonly List<Jump> jumps;
        public readonly List<Block> blocks;

        public List<BlockRange> blockRanges; //set in DecompilePrototype

        public enum Markers
        {
            isNotAdjacent,
            isAdjacent,
            isLoopStart,
            isLoopBody
        };

        public ControlFlowGraph(List<Jump> jumps, List<Block> blocks)
        {
            adj = new byte[blocks.Count, blocks.Count];
            this.jumps = jumps;
            this.blocks = blocks;
            blockRanges = new List<BlockRange>();

            InitializeMatrix();

            foreach (Jump j in jumps)
                SetBlockAdjacencyByJump(j);

            SetBlockAdjacenciesByBlockIndex();
            FindAndMarkLoops();
            DebugAdjMatrix();
        }

        public Block GetJumpBlockTargetByJIndex(int jindex)
        {
            foreach (Jump j in jumps)
                if (j.index == jindex)
                    return j.TargetedBlock;
            throw new Exception("Targeted block not found.");
        }

        private void DebugAdjMatrix()
        {
            //FileManager.ClearDebug();
            FileManager.WriteDebug("\n");
            for (int i = 0; i < adj.GetLength(0); i++)
                for (int j = 0; j < adj.GetLength(1); j++)
                    if (adj[i, j] >= 1 && i != j) //blocks can't point to themselves by definition.
                        FileManager.WriteDebug("iBlock[" + i + "] -> jBlock[" + j + "]");

            FileManager.WriteDebug("\n\n");
            for (int i = 0; i < adj.GetLength(0); i++)
            {
                for (int j = 0; j < adj.GetLength(1); j++)
                    FileManager.WriteDebug(adj[i, j] + " ");
                FileManager.WriteDebug("\n");
            }
        }

        private void FindAndMarkLoops()
        {
            for (int i = 0; i < adj.GetLength(0); i++)
                for (int j = 0; j < adj.GetLength(1); j++)
                    if (adj[i, j] == 1 && i >= j)
                    {
                        adj[i, j] = (byte)Markers.isLoopBody;
                        adj[j, j] = (byte)Markers.isLoopStart;
                    }
        }

        private void SetBlockAdjacencyByJump(Jump j)
        {
            int b1 = FindBlockIndexByInstructionIndex(j.index);

            if (b1 == -1)
                throw new Exception("Cfg:Cfg:: Jump does not exist in any block.");

            if (b1 == -2) //-2 is a flag for the very first jump at the top of the file which *always* points to block 0.
                return;

            int b2 = j.TargetedBlock.blockIndex;
            adj[b1, b2] = (byte)Markers.isAdjacent;
        }

        private void SetBlockAdjacenciesByBlockIndex() //bug here apparently.
        {
            for (int i = 0; i < adj.GetLength(0); i++)
            {
                bool empty = true;

                for (int j = 0; j < adj.GetLength(1); j++)
                    if (adj[i, j] >= 1)
                        empty = false;

                if (empty && i + 1 < adj.GetLength(1))
                    adj[i, i + 1] = 1;
            }
        }

        private void InitializeMatrix()
        {
            for (int i = 0; i < adj.GetLength(0); i++)
                for (int j = 0; j < adj.GetLength(1); j++)
                    adj[i, j] = (byte)Markers.isNotAdjacent;
        }

        public int[] GetChildren(Block b)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < adj.GetLength(1); i++)
                if (adj[b.blockIndex, i] == 1)
                    result.Add(i);
            return result.ToArray();
        }

        public Block[] GetChildBlocks(Block b)
        {
            int[] childIndicies = GetChildren(b);
            Block[] childBlocks = new Block[childIndicies.Length];
            for (int i = 0; i < childIndicies.Length; i++)
                childBlocks[i] = blocks[childIndicies[i]];
            return childBlocks;
        }

        public bool IsBlockLoopStart(Block b)
        {
            return adj[b.blockIndex, b.blockIndex] == 2;
        }

        public int GetParent(Block b)
        {
            for (int i = 0; i < adj.GetLength(0); i++)
                if (adj[i, b.blockIndex] >= 1)
                    return i;
            return -1;
        }

        public Block GetParentBlock(Block b)
        {
            int i = GetParent(b);
            if (i >= 0)
                return blocks[i];
            else return null;
        }

        public int FindBlockIndexByInstructionIndex(int index)
        {
            if (index == -1)
                return -2; //returns -2 in the event that the jump is the very first jump that was artifically created at the top of the file.
            for (int i = 0; i < blocks.Count; i++)
                if (blocks[i].InstructionExists(index) != -1)
                    return blocks[i].blockIndex;
            return -1;
        }

        public Block FindBlockByInstructionIndex(int index)
        {
            return blocks[FindBlockIndexByInstructionIndex(index)];
        }
    }
}
