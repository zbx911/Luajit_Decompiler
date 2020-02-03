using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis.Constants;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;
using Luajit_Decompiler.dec;

namespace Luajit_Decompiler.dec.Structures
{
    class Block
    {
        public int sIndex; //start of the block. (Relative to the asm lines).
        public int eIndex; //end of block.
        public List<BytecodeInstruction> bcis; //all the bytecode instructions in a block.
        public string label;

        private bool finalized = false; //for error checking.
        private Prototype pt;
        private int nameIndex; //used for labeling blocks.

        public Block(int sIndex, int nameIndex, Prototype pt)
        {
            this.sIndex = sIndex;
            this.nameIndex = nameIndex;
            label = "Block[" + nameIndex + "]";
            this.pt = pt;
        }

        public void Finalize(int eIndex)
        {
            this.eIndex = eIndex;
            bcis = new List<BytecodeInstruction>();
            for (int i = sIndex; i < eIndex; i++)
                bcis.Add(pt.bytecodeInstructions[i]);
            finalized = true;
        }

        /// <summary>
        /// Removes duplicate information between a range of indicies.
        /// </summary>
        /// <param name="rMin">Starting BCI index of duplicate info.</param>
        /// <param name="rMax">Ending BCI Index of duplicate info.</param>
        /// <param name="index">Starting index of duplicate info relative to this block.</param>
        public void RemoveDuplicateInfo(int rMin, int rMax, int index)
        {
            int dist = Math.Abs(rMax - rMin);
            bcis.RemoveRange(index, dist);
        }

        /// <summary>
        /// Returns index if the index exists within this block. Returns -1 if it doesn't exist.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public int IndexExists(int index)
        {
            int result = -1;

            for (int i = 0; i < bcis.Count; i++)
                if (bcis[i].index == index)
                    return i;

            return result;
        }

        /// <summary>
        /// Returns the block's name index if the index of an instruction exists within the block. Otherwise, returns -1.
        /// </summary>
        /// <returns></returns>
        public int InstructionExists(int index)
        {
            if (IndexExists(index) != -1)
                return nameIndex;
            return -1;
        }

        /// <summary>
        /// Changes both the name index of this block and the label to match.
        /// </summary>
        /// <param name="index"></param>
        public void ChangeLabel(int index)
        {
            nameIndex = index;
            label = label = "Block[" + nameIndex + "]";
        }

        public int GetNameIndex()
        {
            return nameIndex;
        }

        public override string ToString()
        {
            if (!finalized)
                throw new Exception("Error. Block not finalized.");
            StringBuilder res = new StringBuilder();
            res.AppendLine(label);
            foreach (BytecodeInstruction bci in bcis)
                res.AppendLine(bci.index + ":" + bci.ToString());
            return res.ToString();
        }
    }
}
