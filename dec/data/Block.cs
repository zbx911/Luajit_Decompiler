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
        public readonly int index;
        public bool visited = false;

        private bool finalized = false;
        private Prototype pt;


        public Block(int startIndex, int index, Prototype pt)
        {
            this.startIndex = startIndex;
            this.index = index;
            label = "Block[" + index + "]";
            this.pt = pt;
        }

        public void Finalize(int endIndex)
        {
            this.endIndex = endIndex;
            int instLen = endIndex - startIndex;
            bytecodeInstructions = new BytecodeInstruction[instLen];

            for (int i = startIndex, j = 0; i < endIndex; i++, j++)
                bytecodeInstructions[j] = pt.bcis[i];

            finalized = true;
        }

        //Note: This only works assuming that len-1 and len-2 are the only positions to have jump containing instructions.
        //returns the condition, if any, of the current block.
        public BytecodeInstruction GetConditional()
        {
            return GetJumpInstruction(bytecodeInstructions.Length - 2);
        }

        //returns the jump paired with the condition, if any, of the current block.
        public BytecodeInstruction GetConditionalJump()
        {
            return GetJumpInstruction(bytecodeInstructions.Length - 1);
        }

        private BytecodeInstruction GetJumpInstruction(int index)
        {
            BytecodeInstruction bci = bytecodeInstructions[index];
            if (DecompilePrototype.IdentifyJumpOrReturn(bci) <= 0)
                return null;
            return bci;
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
                return this.index;
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
            return res.ToString();
        }
    }
}
