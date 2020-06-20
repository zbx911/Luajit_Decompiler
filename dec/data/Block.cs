using System;
using System.Collections.Generic;
using System.Text;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.lir;

namespace Luajit_Decompiler.dec.data
{
    class Block
    {
        public int sIndex; //start of the block. (Relative to the asm lines).
        public int eIndex; //end of block.
        public List<IntegratedInstruction> iis;
        public string label;
        public bool hasJmp = false;
        public bool hasCondi = false;

        private bool finalized = false; //for error checking.
        private Prototype pt;
        private readonly int nameIndex; //used for labeling blocks.

        public Block(int sIndex, int nameIndex, Prototype pt)
        {
            this.sIndex = sIndex;
            this.nameIndex = nameIndex;
            label = "Block[" + nameIndex + "]";
            this.pt = pt;
        }

        public void Finalize(int eIndex) //finalize to integrated instructions
        {
            this.eIndex = eIndex;
            iis = new List<IntegratedInstruction>();
            IRIMap map = new IRIMap();
            for(int i = sIndex; i < eIndex; i++)
            {
                BytecodeInstruction bci = pt.bytecodeInstructions[i];
                IntegratedInstruction ii = new IntegratedInstruction(map.Translate(bci.opcode), bci.opcode, bci.index, bci.regA, bci.regB, bci.regC);
                if (ii.iROp == IRIMap.IRMap.Goto) //goto = jump
                    hasJmp = true;
                if (ii.iROp == IRIMap.IRMap.Eval)
                    hasCondi = true;
                iis.Add(ii);
            }
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

            for (int i = 0; i < iis.Count; i++)
                if (iis[i].originalIndex == index)
                    return i;

            return result;
        }

        /// <summary>
        /// returns the block's name index if the index of an instruction exists within the block. otherwise, returns -1.
        /// </summary>
        /// <returns></returns>
        public int InstructionExists(int index)
        {
            if (IndexExists(index) != -1)
                return nameIndex;
            return -1;
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
            foreach (IntegratedInstruction ii in iis)
                res.AppendLine(ii.originalIndex + ":" + ii.ToString());
            return res.ToString();
        }
    }
}
