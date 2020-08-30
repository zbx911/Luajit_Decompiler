using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.gir;
using Luajit_Decompiler.dis;
using System.Collections.Generic;
using Luajit_Decompiler.dis.consts;
using Luajit_Decompiler.dec.lir;
using System.Text;
using static Luajit_Decompiler.dec.lir.IntegratedInstruction;
using System;

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

    #region To-Do
    //Implement Unary operations and Const operations.
    //Double check that the GTGet states do not actually require any lua to be written.
    //Optional: We can probably track slots by keeping track of the A register to see if we need to create a new slot/variable or not.
    #endregion

    class TLuaState
    {
        public readonly Prototype pt; //reference to the prototype we are working with
        public readonly Cfg cfg; //reference to a prototype's control flow graph.
        public readonly List<Block> blocks; //reference to a prototype's blocks which contain its integrated instructions.
        public readonly BaseConstant[] _G; //reference to the prototype's global constants table.   
        public readonly BaseConstant[] _U; //value of upvalues in each prototype.

        public int indent; //current nest level. aka the # of tab indentations. Fetch this from the control flow graph.
        public List<BaseConstant> slots = new List<BaseConstant>(); //Slots act sort of like registers as temporary memory locations for an operation to use.
        public IntegratedInstruction curII; //Current integrated instruction we are working with.
        public List<string> varNames; //names of variables from the prototype.
        public List<string> decompLines; //buffer for the states to write to. Per prototype. Each index represents an individual line in the decomp. Allows for in-lining some things.
        public Registers regs; //registers for the state to look at and use.

        private Block curBlock; //current block we are looking at
        private int bIndex; //current instruction index relative to the block's IIs.
        private int varCount;
        private int curSlot; //current slot we are working with.

        public TLuaState(ref Prototype pt, ref Cfg cfg, ref List<Block> blocks, ref List<string> decompLines)
        {
            this.pt = pt;
            this.cfg = cfg;
            this.blocks = blocks;
            _G = pt.constantsSection.ToArray();
            _U = GetUpvalues().ToArray();
            curBlock = blocks[0];
            bIndex = -1; //we start here due to NextII.
            this.decompLines = decompLines; //*hopefully* stores it by reference.
            varNames = pt.variableNames; //may be an empty list.
            varCount = 0;
            indent = 0;
            curSlot = -1;
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
            regs = curII.registers;
        }

        /// <summary>
        /// Checks to see if a slot needs to be added for a variable.
        /// Returns -- 1: Successfully added variable in correct slot. 0: Slot already exists. -1: Slot did not get added to correct location and value has been removed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        public int CheckAddSlot(BaseConstant value, int slotIndex)
        {
            if(slotIndex > curSlot)
            {
                slots.Add(value);
                if (value.GetValue() != slots[regs.regA].GetValue())
                {
                    slots.Remove(value);
                    return -1;
                }
                curSlot++;
                return 1;
            }
            else
                return 0;
        }

        /// <summary>
        /// Returns an incrementing variable for nameless variables. (Or when debug info is stripped).
        /// </summary>
        /// <returns></returns>
        private string GenerateVarName()
        {
            string result = "var" + varCount;
            varCount++;
            return result;
        }

        /// <summary>
        /// Gets variable name based on given register as index. If none exist at the register, then we insert a name at that slot.
        /// </summary>
        /// <returns></returns>
        public string GetVariableName(int slot)
        {
            if (slot < varNames.Count) //return it if possible.
                return varNames[slot]; //I think it is mapped by index...

            //No variable in given slot. Create a slot for it with that variable name.
            string name = GenerateVarName();
            slots.Add(new BaseConstant()); //give it a blank constant for now...
            varNames.Insert(slot, name);
            return name;
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
            if (uv.TableLocation == 192) 
                return pt.parent.constantsSection[uv.TableIndex];
            return RecursiveGetUpvalue(pt.parent, pt.parent.upvalues[uv.TableIndex]);
        }
    }
}
