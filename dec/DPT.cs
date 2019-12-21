using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;

namespace Luajit_Decompiler.dec
{
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
            CreateIR();
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
            FinalizeTargets();
        }

        /// <summary>
        /// Sets end index for each jump target.
        /// </summary>
        private void FinalizeTargets()
        {
            for (int i = 0; i < jumps.Count; i++)
            {
                if (i + 1 >= jumps.Count) //no next jump, then set target of last jump to end.
                {
                    jumps[i].target.Finalize(jumps.Count - 1);
                    break;
                }
                else
                {
                    jumps[i].target.Finalize(jumps[i + 1].target.sIndex);
                }
            }
        }

        /// <summary>
        /// Correctly orders certain method calls from 1 method call.
        /// </summary>
        private void CreateIR()
        {
            BlockPrototype(); //#1
            SimplifyLIR(); //#2
            CreateBlockList(); //#3
            //SimplifyBlocks(); //#4
            //Create graph...
        }

        /// <summary>
        /// Simplifies linear intermediate representation using some created opcodes.
        /// </summary>
        private void SimplifyLIR() //O((2N^2) * M) = O(N^2)? Perhaps rethink this method later for efficiency's sake. N = # jumps, M = # of bcis in jump N.
        {
            for (int i = 0; i < jumps.Count; i++)
            {
                ref List<BytecodeInstruction> targetBCIs = ref jumps[i].target.bcis;
                for (int j = 0; j < targetBCIs.Count; j++)
                {
                    int check = CheckCJR(targetBCIs[j]);

                    //# compound IF IR. if the assumption that after every comparison op exists a jump op is false, use the commented implementation @ bottom of file instead of this implementation.
                    if (check == 3)
                    {
                        //take this instruction plus next and merge. (ISGE->JMP)
                        BytecodeInstruction ir = new BytecodeInstruction(OpCodes._if, targetBCIs[j].index);
                        ir.AddRegister((byte)FindJumpAtIndex(targetBCIs[j].index).target.nameIndex); //this register will hold block index of target if true. This can throw issues if nameIndex > 255 for a prototype.
                        ir.AddRegister((byte)FindJumpAtIndex(targetBCIs[j + 1].index).target.nameIndex); //this register holds the target block if the jump is false.
                        comparisonBCIs.Add(targetBCIs[j]);
                        ir.AddRegister(comparisonBCIs.IndexOf(targetBCIs[j])); //used for keeping track of index of original conditional expression.
                        targetBCIs[j] = ir;

                        //removed merged bci
                        BytecodeInstruction rem = new BytecodeInstruction(OpCodes.removed, targetBCIs[j + 1].index);
                        rem.AddRegister(0);
                        rem.AddRegister(0);
                        rem.AddRegister(0);
                        targetBCIs[j + 1] = rem;
                    }
                    else if (check == 1)
                    {
                        BytecodeInstruction ir = new BytecodeInstruction(OpCodes._goto, targetBCIs[j].index);
                        ir.AddRegister((byte)FindJumpAtIndex(targetBCIs[j].index).target.nameIndex);
                        ir.AddRegister(255);
                        ir.AddRegister(255);
                        targetBCIs[j] = ir;
                    }
                    //#
                }
            }
        }

        /// <summary>
        /// Block index + 1 = Index in Block List.
        /// </summary>
        private void CreateBlockList()
        {
            blocks = new List<Block>();
            foreach (Jump j in jumps)
                blocks.Add(j.target);
        }

        /// <summary>
        /// ****TRY USING .CONTAINS IN BCI LIST?
        /// Removes blocks that are a subset of another block to eliminate redundancy.
        /// </summary>
        private void SimplifyBlocks() //A fairly CPU intensive algorithm. Might want to make this more efficient later.
        {
            for (int i = 0; i < jumps.Count; i++)
            {
                Block b0 = jumps[i].target;
                for(int j = 0; j < b0.bcis.Count; j++)
                {
                    BytecodeInstruction bci = b0.bcis[i];
                    if (bci.opcode == OpCodes._if)
                    {
                        int t1 = bci.registers[0]; //true block target
                        int t2 = bci.registers[1]; //false block target
                        t1++; //increment to access it in the block list.
                        t2++;
                        Block b1 = blocks[t1];
                        Block b2 = blocks[t2];
                        if (j + 2 >= b0.bcis.Count) //no subsets to check
                            continue;
                        for (int k = j + 2; k < b0.bcis.Count; k++) //+2 from the _if opcode = bci after the _if.
                        {
                              
                        }
                    }
                    else continue;
                }
            }
        }

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
