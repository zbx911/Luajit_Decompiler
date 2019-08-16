using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;

namespace Luajit_Decompiler.dec
{
    class DecPrototypes
    {
        private StringBuilder fileSource = new StringBuilder(); //source code for the entire file.

        #region Per Prototype
        public static Prototype pt; //reference to current prototype.
        public static List<Jump> jumps; //jumps and their associated targets.
        public static List<BytecodeInstruction> comparisonBCIs;
        #endregion

        /// <summary>
        /// Decompiles an entire file's prototypes.
        /// </summary>
        /// <param name="name">Name of the entire file.</param>
        /// <param name="pts">List containing that file's prototypes.</param>
        public DecPrototypes(string name, List<Prototype> pts)
        {
            StringBuilder res = new StringBuilder();
            res.AppendLine("--Lua File Name: " + name);
            for (int i = pts.Count; i > 0; i--) //We go backwards here because the 'main' proto is always the last one and will have the most prototype children.
            {
                //init
                pt = pts[i - 1];
                comparisonBCIs = new List<BytecodeInstruction>();
                jumps = new List<Jump>();

                //IR creation.
                CreateIR();
                //PtGraph graph = new PtGraph();

                #region debugging
                StringBuilder dbg = new StringBuilder();

                foreach(Jump j in jumps)
                {
                    //dbg.AppendLine("Jump@" + j.index + " Type: " + j.jumpType + " Block Starts: " + j.target.sIndex);
                    dbg.AppendLine("Jump@" + j.index + "; Target =>\r\n" + j.target.ToString());
                }
                FileManager.ClearDebug();
                FileManager.WriteDebug(dbg.ToString());
                #endregion
            }
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
            Jump top = new Jump(jmpTop, 1, -1);
            jumps.Add(top);

            //get jump targets
            for (int i = 0; i < ptBcis.Count; i++)
            {
                int check = CheckCJR(ptBcis[i]);
                if (check == 1 || check == 3) //jmp or comparison
                {
                    Jump j = new Jump(ptBcis[i], check, name);
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

        private void CreateIR()
        {
            BlockPrototype(); //#1
            PhaseOneIRTranslate(); //#2
            //Create graph...
        }

        /// <summary>
        /// Phase one of IR translation. Turn any comparsion opcodes and jumps to their linear IR equivalent.
        /// </summary>
        private void PhaseOneIRTranslate() //O((2N^2) * M) = O(N^2)? Perhaps rethink this method later for efficiency's sake. N = # jumps, M = # of bcis in jump N.
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
                        ir.AddRegister((byte)FindJumpAtIndex(targetBCIs[j+1].index).target.nameIndex); //this register holds the target block if the jump is false.
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
        /// Check for Condi, Jump, or Ret opcodes.
        /// </summary>
        /// <param name="bci"></param>
        /// <returns></returns>
        public static int CheckCJR(BytecodeInstruction bci)
        {
            switch(bci.opcode)
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

        public static Jump FindJumpAtIndex(int index)
        {
            for (int i = 0; i < jumps.Count; i++)
                if (jumps[i].index == index)
                    return jumps[i];
            throw new Exception("Jump not found.");
        }

        public override string ToString()
        {
            return fileSource.ToString();
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