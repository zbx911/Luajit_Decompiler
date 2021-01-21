using System;
using System.Text;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.data
{
    class Block
    {
        public int startIndex;
        public int endIndex;
        public BytecodeInstruction[] bytecodeInstructions;
        public string label;
        public readonly int blockIndex;
        public bool visited = false;
        public int indent = 0;
        public bool needsEndStatement = false;

        private bool finalized = false;
        private Prototype pt;


        public Block(int startIndex, int blockIndex, Prototype pt)
        {
            this.startIndex = startIndex;
            this.blockIndex = blockIndex;
            label = "Block[" + blockIndex + "]";
            this.pt = pt;
        }

        public void Finalize(int endIndex)
        {
            this.endIndex = endIndex;
            int instLen = endIndex - startIndex;
            bytecodeInstructions = new BytecodeInstruction[instLen];

            for (int i = startIndex, j = 0; i < endIndex; i++, j++)
                bytecodeInstructions[j] = pt.bytecodeInstructions[i];

            finalized = true;
        }

        /// <summary>
        /// Returns index if the index exists within this block. Returns -1 if it doesn't exist.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public int IndexExists(int index)
        {
            int result = -1;

            for (int i = 0; i < bytecodeInstructions.Length; i++)
                if (bytecodeInstructions[i].bciIndexInPrototype == index)
                    return i;

            return result;
        }

        /// <summary>
        /// Returns the block index if the index of an instruction exists within the block. otherwise, returns -1.
        /// </summary>
        /// <returns></returns>
        public int InstructionExists(int index)
        {
            if (IndexExists(index) != -1)
                return blockIndex;
            return -1;
        }

        public override string ToString()
        {
            if (!finalized)
                throw new Exception("Error. Block not finalized.");

            StringBuilder res = new StringBuilder();
            res.AppendLine(label);
            foreach (BytecodeInstruction bci in bytecodeInstructions)
                res.AppendLine(bci.bciIndexInPrototype + ":" + bci.ToString());
            res.AppendLine();
            return res.ToString();
        }
    }
}
