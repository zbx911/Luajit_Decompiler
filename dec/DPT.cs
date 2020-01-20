using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;
using Luajit_Decompiler.dec.lir;

namespace Luajit_Decompiler.dec
{
    /// <summary>
    /// TODO: Bugfix on line 75.
    /// </summary>
    class DPT //Decompile Prototype
    {
        private Prototype pt;
        private List<BytecodeInstruction> ptBcis;
        public List<Jump> jumps; //public for now for debug purposes.
        private List<BytecodeInstruction> comparisonBCIs;
        private List<Block> blocks;

        public DPT(Prototype pt)
        {
            this.pt = pt;
            ptBcis = pt.bytecodeInstructions;
            jumps = new List<Jump>();
            comparisonBCIs = new List<BytecodeInstruction>();
            blocks = new List<Block>();
            CreateIR();
        }

        /// <summary>
        /// Correctly orders certain method calls from 1 method call.
        /// </summary>
        private void CreateIR()
        {
            #region debugging map
            //IRIMap ir = new IRIMap();
            //FileManager.ClearDebug();
            //var ops = Enum.GetValues(typeof(OpCodes));
            //foreach(OpCodes op in ops)
            //    FileManager.WriteDebug(ir.Translate(op).ToString());
            #endregion

            BlockPrototype(); //#1
            //see plan in IRImap
        }

        /// <summary>
        /// Gets jump targets and finalizes those blocks.
        /// </summary>
        private void BlockPrototype()
        {
            List<BytecodeInstruction> ptBcis = pt.bytecodeInstructions;

            //find condi and jump treat them as jumps. pass #1 of bytecode
            int name = 0;

            //make a jmp bci to top of file
            BytecodeInstruction jmpTop = new BytecodeInstruction(OpCodes.JMP, -1);
            jmpTop.AddRegister(0);
            jmpTop.AddRegister(0);
            jmpTop.AddRegister(128);
            Jump top = new Jump(jmpTop, 1, -1, pt);
            jumps.Add(top);

            //get jump targets
            for (int i = 0; i < ptBcis.Count; i++)
            {
                int check = CheckCJR(ptBcis[i]);
                if (check == 1 || check == 3) //jmp or comparison
                {
                    Jump j = new Jump(ptBcis[i], check, name, pt);
                    jumps.Add(j);
                    name++;
                }
            }
            //FileManager.ClearDebug();
            //foreach (Jump j in jumps)
            //    FileManager.WriteDebug("Jump Index: " + j.index + " -> " + j.target);

            //All blocks now have a starting index.

            //Block = (J).target to (J+1).target - 1
            //if (J).target > (J+1).target - 1 then do not make block, push J to stack for second pass. (points to block not yet encountered).
            int blockName = 0;
            Stack<Jump> blockSkips = new Stack<Jump>();
            for(int i = 0; i < jumps.Count; i++)
            {
                //looping jump:: jumps[i].index > jumps[i].target
                Block b;
                //if (i + 1 >= jumps.Count) //BUG: assumes the last jump is the jump to the return statement.
                //{
                //    b = new Block(jumps[i].target, blockName, pt);
                //    b.Finalize(ptBcis.Count);
                //    blocks.Add(b);
                //    jumps[i].Block = b;
                //    break;
                //}
                if (jumps[i].target > jumps[i + 1].target - 1)
                {
                    blockSkips.Push(jumps[i]);
                    continue;
                }
                else
                {
                    b = new Block(jumps[i].target, blockName, pt);
                    b.Finalize(jumps[i + 1].target);
                    blocks.Add(b);
                    jumps[i].Block = b;
                    blockName++;
                }
            }

            //second pass to deal with the jumps that went a bit too far.
            while (blockSkips.Count > 0)
            {
                Jump j = blockSkips.Pop();
                bool success = false;
                foreach(Block b in blocks)
                {
                    if (b.sIndex == j.target)
                    {
                        j.Block = b;
                        success = true;
                        break;
                    }
                }
                if (!success)
                    throw new Exception("Block not found.");
            }

            FileManager.ClearDebug();
            foreach (Jump j in jumps)
                FileManager.WriteDebug("Jump Index: " + j.index + " -> " + j.Block.label);
            FileManager.WriteDebug("\r\n");
            FileManager.WriteDebug("------------------------------");
            FileManager.WriteDebug("\r\n");
            foreach (Block b in blocks)
                FileManager.WriteDebug(b.ToString());

            //Jump Index: -1 -> 0
            //Jump Index: 2 -> 4 //0 to 3 is a block.
            //Jump Index: 3 -> 7 //4 to 6 is a block
            //Jump Index: 9 -> 11 //7 to 10 is a block.
            //Jump Index: 10 -> 15 //11 to 14
            //Jump Index: 14 -> 18 //15 to 17
            //Jump Index: 20 -> 22 //18 to 21
            //Jump Index: 21 -> 26 //22 to 25
            //Jump Index: 25 -> 37 //26 to 36 (J)
            //Jump Index: 28 -> 30 //37 to 29(J+1) -- skip block since j.target > j+1.target - 1. means that (J).target is a jump that targets a block not yet encountered. push jump to a stack?
            //Jump Index: 29 -> 34 
            //Jump Index: 33 -> 37
            //Jump Index: 39 -> 41 
            //Jump Index: 40 -> 49
            //Jump Index: 43 -> 45
            //Jump Index: 44 -> 49
            //Jump Index: 48 -> 64
            //Jump Index: 51 -> 53
            //Jump Index: 52 -> 57
            //Jump Index: 55 -> 57
            //Jump Index: 56 -> 61
            //Jump Index: 60 -> 64
        }

        /// <summary>
        /// Simplifies linear intermediate representation using some created opcodes.
        /// </summary>
        //private void SimplifyLIR() //O((2N^2) * M) = O(N^2)? Perhaps rethink this method later for efficiency's sake. N = # jumps, M = # of bcis in jump N.
        //{
        //    for (int i = 0; i < jumps.Count; i++)
        //    {
        //        ref List<BytecodeInstruction> targetBCIs = ref jumps[i].target.bcis;
        //        for (int j = 0; j < targetBCIs.Count; j++)
        //        {
        //            int check = CheckCJR(targetBCIs[j]);

        //            //# compound IF IR. if the assumption that after every comparison op exists a jump op is false, use the commented implementation @ bottom of file instead of this implementation.
        //            if (check == 3)
        //            {
        //                //take this instruction plus next and merge. (ISGE->JMP)
        //                BytecodeInstruction ir = new BytecodeInstruction(OpCodes._if, targetBCIs[j].index);
        //                ir.AddRegister((byte)FindJumpAtIndex(targetBCIs[j].index).target.nameIndex); //this register will hold block index of target if true. This can throw issues if nameIndex > 255 for a prototype.
        //                ir.AddRegister((byte)FindJumpAtIndex(targetBCIs[j + 1].index).target.nameIndex); //this register holds the target block if the jump is false.
        //                comparisonBCIs.Add(targetBCIs[j]);
        //                ir.AddRegister(comparisonBCIs.IndexOf(targetBCIs[j])); //used for keeping track of index of original conditional expression.
        //                targetBCIs[j] = ir;

        //                //removed merged bci
        //                BytecodeInstruction rem = new BytecodeInstruction(OpCodes.removed, targetBCIs[j + 1].index);
        //                rem.AddRegister(0);
        //                rem.AddRegister(0);
        //                rem.AddRegister(0);
        //                targetBCIs[j + 1] = rem;
        //            }
        //            else if (check == 1)
        //            {
        //                BytecodeInstruction ir = new BytecodeInstruction(OpCodes._goto, targetBCIs[j].index);
        //                ir.AddRegister((byte)FindJumpAtIndex(targetBCIs[j].index).target.nameIndex);
        //                ir.AddRegister(255);
        //                ir.AddRegister(255);
        //                targetBCIs[j] = ir;
        //            }
        //            //#
        //        }
        //    }
        //}

        /// <summary>
        /// ****TRY USING .CONTAINS IN BCI LIST?
        /// Removes blocks that are a subset of another block to eliminate redundancy.
        /// </summary>
        //private void SimplifyBlocks() //A fairly CPU intensive algorithm. Might want to make this more efficient later.
        //{
        //    for (int i = 0; i < jumps.Count; i++)
        //    {
        //        Block b0 = jumps[i].target;
        //        for(int j = 0; j < b0.bcis.Count; j++)
        //        {
        //            BytecodeInstruction bci = b0.bcis[i];
        //            if (bci.opcode == OpCodes._if)
        //            {
        //                int t1 = bci.registers[0]; //true block target
        //                int t2 = bci.registers[1]; //false block target
        //                t1++; //increment to access it in the block list.
        //                t2++;
        //                Block b1 = blocks[t1];
        //                Block b2 = blocks[t2];
        //                if (j + 2 >= b0.bcis.Count) //no subsets to check
        //                    continue;
        //                for (int k = j + 2; k < b0.bcis.Count; k++) //+2 from the _if opcode = bci after the _if.
        //                {
                              
        //                }
        //            }
        //            else continue;
        //        }
        //    }
        //}

        /// <summary>
        /// Check for Condi, Jump, or Ret opcodes.
        /// </summary>
        /// <param name="bci"></param>
        /// <returns></returns>
        public static int CheckCJR(BytecodeInstruction bci)
        {
            switch (bci.opcode)
            {
                case OpCodes.JMP:
                    return 1; //jump
                case OpCodes.RET:
                case OpCodes.RET0:
                case OpCodes.RET1:
                case OpCodes.RETM:
                    return 2; //return
                case OpCodes.ISEQN:
                case OpCodes.ISEQP:
                case OpCodes.ISEQS:
                case OpCodes.ISEQV:
                case OpCodes.ISGE:
                case OpCodes.ISGT:
                case OpCodes.ISLE:
                case OpCodes.ISLT:
                case OpCodes.ISNEN:
                case OpCodes.ISNEP:
                case OpCodes.ISNES:
                case OpCodes.ISNEV:
                    return 3; //conditional
                default:
                    return -1; //not condi/jmp/ret
            }
        }

        public Jump FindJumpAtIndex(int index)
        {
            for (int i = 0; i < jumps.Count; i++)
                if (jumps[i].index == index)
                    return jumps[i];
            throw new Exception("Jump not found.");
        }

        public override string ToString()
        {
            return ""; //return prototype source here.
        }

        //# implementation for non-compound _if statements.
        //if (check == 3 || check == 1)
        //{
        //    BytecodeInstruction ir;
        //    if (check == 3)
        //        ir = new BytecodeInstruction(OpCodes._if, targetBCIs[j].index);
        //    else
        //        ir = new BytecodeInstruction(OpCodes._goto, targetBCIs[j].index);

        //    ir.AddRegister((byte)FindJumpAtIndex(targetBCIs[j].index).target.nameIndex); //this register will hold block index of target if true. This can throw issues if nameIndex > 255 for a prototype.
        //    ir.AddRegister(0);
        //    ir.AddRegister(0);
        //    targetBCIs[j] = ir;
        //}
        //#
    }
}
