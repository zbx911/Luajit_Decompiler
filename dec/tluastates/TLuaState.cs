﻿using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.gir;
using Luajit_Decompiler.dis;
using System.Collections.Generic;
using Luajit_Decompiler.dis.consts;
using Luajit_Decompiler.dec.lir;

namespace Luajit_Decompiler.dec.tluastates
{
    public enum State
    {
        init,
        check,
        setslot, //operation involving the setting of a slot.
        binop, // + - * / etc... 
        eval,
        loop
    }

    class TLuaState
    {
        public readonly Prototype pt; //reference to the prototype we are working with
        public readonly Cfg cfg; //reference to a prototype's control flow graph.
        public readonly List<Block> blocks; //reference to a prototype's blocks which contain its integrated instructions.
        public readonly BaseConstant[] _G; //reference to the prototype's global constants table.   
        public readonly BaseConstant[] _U; //value of upvalues in each prototype.


        public int indent; //current nest level. aka the # of tab indentations. Fetch this from the control flow graph.
        public BaseConstant[] slots = new BaseConstant[3]; //0, 1, 2 are the slots in luajit. They act sort of like registers as temporary memory locations for an operation to use.
        public IntegratedInstruction curII; //Current integrated instruction we are working with.
        public Dictionary<string, BaseConstant> variables; //keeping track of variables as we discover them.

        private Block curBlock; //current block we are looking at
        private int bIndex; //current instruction index relative to the block's IIs.

        public TLuaState(ref Prototype pt, ref Cfg cfg, ref List<Block> blocks)
        {
            this.pt = pt;
            this.cfg = cfg;
            this.blocks = blocks;
            _G = pt.constantsSection.ToArray();
            _U = GetUpvalues().ToArray();
            curBlock = blocks[0];
            bIndex = -1; //we start here due to NextII.
            variables = new Dictionary<string, BaseConstant>();
            NextII();
    }

        /// <summary>
        /// Advances the current integrated instruction to the next. EndOfIIStream is the IROP flag for end of instructions for this prototype.
        /// </summary>
        public void NextII()
        {
            bIndex++;
            if (bIndex >= curBlock.iis.Count) //advance to next block if necessary.
            {
                if (curBlock.GetNameIndex() + 1 >= blocks.Count) //no more instructions.
                {
                    IntegratedInstruction end = new IntegratedInstruction(IRMap.EndOfIIStream, OpCodes.RETM, -1, 0, 0, 0); //basically a flag instruction
                    curII = end;
                }
                else
                {
                    curBlock = blocks[curBlock.GetNameIndex() + 1];
                    bIndex = 0;
                    curII = curBlock.iis[bIndex];
                }
            }
            curII = curBlock.iis[bIndex];
        }

        private List<BaseConstant> GetUpvalues()
        {
            List<BaseConstant> result = new List<BaseConstant>();
            foreach (UpValue uv in pt.upvalues)
                result.Add(RecursiveGetUpvalue(pt, uv));
            return result;
        }

        private BaseConstant RecursiveGetUpvalue(Prototype pt, UpValue uv)
        {
            //apparently the first bit of 192 determines if we look at the constants section table or not. 
            //the second bit of 192 means if it is mutable or not. 1 = immutable upvalue -- whatever that means in terms of upvalues...
            if (uv.tableLocation == 192) 
                return pt.parent.constantsSection[uv.tableIndex];
            return RecursiveGetUpvalue(pt.parent, pt.parent.upvalues[uv.tableIndex]);
        }
    }
}
