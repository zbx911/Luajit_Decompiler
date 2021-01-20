using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.gir;
using Luajit_Decompiler.dis;
using System.Collections.Generic;
using Luajit_Decompiler.dis.consts;
using Luajit_Decompiler.dec.lir;
using System;
using System.Text;

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
    //Table Operations
    //Conditional Statements & Loops
    //Functions and their names. (Mixing of various prototypes, multiple TLuaStates, etc)
    //Low Priority: Redo how we keep track of which variables are local or not -- Concat operation in-lining requires that we remove some lines in which a variable is declared as local.
    #endregion

    class TLuaState
    {
        public readonly Prototype pt; //reference to the prototype we are working with
        public readonly ControlFlowGraph cfg; //reference to a prototype's control flow graph.
        public readonly List<Block> blocks; //reference to a prototype's blocks which contain its integrated instructions.
        //public BaseConstant[] _G; //reference to the prototype's global constants table.   
        public readonly BaseConstant[] _U; //value of upvalues in each prototype.

        //Update: _G is separated into a numeric part and a string part. Tables might be on their own too?
        public List<BaseConstant> num_G;
        public List<CString> string_G;
        public List<CTable> table_G;

        public IntegratedInstruction curII; //Current integrated instruction we are working with.
        public IntegratedInstruction prevII; //Previous integrated instruction.
        public List<string> varNames; //names of variables from the prototype.
        public List<string> decompLines; //buffer for the states to write to. Per prototype. Each index represents an individual line in the decomp. Allows for in-lining some things.
        public InstructionRegisters regs; //registers for the state to look at and use.
        public BaseConstant[] slots; //Keeping track of what is within some slots for some opcodes like LEN which use slot index rather than _G index.

        private Block curBlock; //current block we are looking at
        private int bIndex; //current instruction index relative to the block's IIs.
        private int varCount; //for keeping track of generated variable names
        private int curSlot; //for keeping track of declared variables.

        public TLuaState(ref Prototype pt, ref ControlFlowGraph cfg, ref List<Block> blocks, ref List<string> decompLines)
        {
            this.pt = pt;
            this.cfg = cfg;
            this.blocks = blocks;
            slots = new BaseConstant[pt.frameSize]; //frame size = total # of slots.
            //_G = PrepareGlobals();
            PrepareGlobals();
            _U = GetUpvalues().ToArray();
            curBlock = blocks[0];
            bIndex = -1; //we start here due to NextII.
            this.decompLines = decompLines; //*hopefully* stores it by reference.
            varNames = pt.variableNames; //may be an empty list.
            varCount = 0;
            curSlot = -1;
        }

        /// <summary>
        /// Advances the current integrated instruction to the next. EndOfIIStream is the IROP flag for end of instructions for this prototype.
        /// </summary>
        public void NextII()
        {
            prevII = curII;
            bIndex++;
            if (bIndex >= curBlock.iis.Count) //advance to next block if necessary.
            {
                if (curBlock.GetNameIndex() + 1 >= blocks.Count) //no more instructions.
                {
                    //IntegratedInstruction end = new IntegratedInstruction(IRMap.EndOfIIStream, OpCodes.RET0, -1, 0, 0, 0); //basically a flag instruction
                    IntegratedInstruction end = new IntegratedInstruction(IRMap.EndOfIIStream, new BytecodeInstruction(OpCodes.RET0, -2)); //basically a flag instruction
                    curII = end;
                }
                else
                {
                    curBlock = blocks[curBlock.GetNameIndex() + 1];
                    bIndex = 0;
                    curII = curBlock.iis[bIndex];
                }
            }
            if (curII != null && curII.iROp == IRMap.EndOfIIStream)
                return;
            curII = curBlock.iis[bIndex];
            regs = curII.bci.regs;
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
        /// Attempts to retrieve a variable name from varNames.
        /// Item1:
        /// Returns true if we found one already existing.
        /// Returns false if we had to make one.
        /// Item2:
        /// Variable name.
        /// </summary>
        /// <param name="slotIndex"></param>
        /// <returns></returns>
        public Tuple<bool, string> CheckGetVarName(int slotIndex)
        {
            bool local = slotIndex > curSlot;
            if (local)
                curSlot++;
            if (varNames.Count > slotIndex) //if a varname exists or not
                return new Tuple<bool, string>(local, varNames[slotIndex]);
            else
                return new Tuple<bool, string>(local, GenerateVarName());
        }

        /// <summary>
        /// Checks to see if a variable name is a new variable or not. If it is a new variable, it will append "local " to the line.
        /// </summary>
        /// <param name="dst"></param>
        public void CheckLocal(Tuple<bool, string> dst, ref StringBuilder line)
        {
            if(dst.Item1)
                line.Append("local ");
        }

        /// <summary>
        /// Writes a notification to the decompiled output that an opcode is currently unimplmented.
        /// </summary>
        public void UnimplementedOpcode(IntegratedInstruction ii)
        {
            decompLines.Add("--Opcode not impemented: " + ii.originalOp);
        }

        private void PrepareGlobals() //TODO: Might want to come up with a better solution to this. Basically, _G is in reverse order for all constant types.
        {
            //place into their own individual arrays since LJ accesses constants this way.
            num_G = new List<BaseConstant>();
            string_G = new List<CString>();
            table_G = new List<CTable>();

            foreach(BaseConstant c in pt.constantsSection.ToArray())
            {
                Type t = c.GetType();
                if (t.Equals(typeof(CString)))
                    string_G.Add((CString)c);
                else if (t.Equals(typeof(CTable)))
                    table_G.Add((CTable)c);
                else //numeric most likely
                    num_G.Add(c);
            }

            //reverse the order of strings so that they correctly line up with slot indicies
            string_G.Reverse();
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
