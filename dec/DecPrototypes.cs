using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;
using Luajit_Decompiler.dis.Constants;

namespace Luajit_Decompiler.dec
{
    class DecPrototypes
    {
        private List<Block> ptBlocks; //list of all asm blocks to a single prototype.
        private StringBuilder fileSource = new StringBuilder(); //source code for the entire file.
        private Variables vars; //temp vars for each file.
        private List<Jump> jumps; //jumps per prototype.

        /// <summary>
        /// Decompiles an entire file's prototypes.
        /// </summary>
        /// <param name="name">Name of the entire file.</param>
        /// <param name="pts">List containing that file's prototypes.</param>
        public DecPrototypes(string name, List<Prototype> pts)
        {
            StringBuilder res = new StringBuilder();
            res.AppendLine("--Lua File Name: " + name);
            vars = new Variables(); //temp vars for the file.
            for (int i = pts.Count; i > 0; i--) //We go backwards here because the 'main' proto is always the last one and will have the most prototype children.
            {
                int blockifyIndex = 0;
                ptBlocks = new List<Block>();
                jumps = new List<Jump>();
                BlockifyPT(pts[i - 1], ref blockifyIndex, pts[i - 1].bytecodeInstructions.Count); //Note: After each block's endIndex is a JMP in the prototype asm. (endIndex + 1)
                SliceBlocks(jumps, ptBlocks, pts[i - 1]);
                foreach(Block b in ptBlocks)
                {
                    //DecompileBlock(b, ref vars);
                    if (!b.written) //if it hasn't been written already, write it.
                    {
                        //b.varRef = vars; //should be by reference.
                        //b.ptBlocks = ptBlocks;
                        //b.currentPT = pts[i - 1];
                        fileSource.Append(b.WriteBlock(vars));
                    }
                }
            }
            //foreach (Block b in ptBlocks) //debug
            //    Console.Out.WriteLine(b.ToString()); //debug
            //Console.Read(); //debug
        }

        /// <summary>
        /// Separates the prototype into blocks.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void BlockifyPT(Prototype pt, ref int start, int end)
        {
            Block b = new Block();
            while (start < end)
            {
                b.startB = start; //start of a block by line in asm for reference.
                BytecodeInstruction bci = pt.bytecodeInstructions[start];
                bool isJmpOrRet;
                switch (bci.opcode) //consider storing index of jump/ret in a list
                {
                    case OpCodes.JMP:
                    case OpCodes.RET:
                    case OpCodes.RET0:
                    case OpCodes.RET1:
                    case OpCodes.RETM:
                        isJmpOrRet = true;
                        break;
                    default:
                        isJmpOrRet = false;
                        break;
                }
                if (isJmpOrRet) 
                {
                    if(bci.opcode == OpCodes.JMP)
                        jumps.Add(new Jump(bci, start)); //add the jump
                    b.endB = start - 1; //terminate block at instruction above JMP.
                    b.ptBlocks = ptBlocks;
                    b.varRef = vars;
                    b.currentPT = pt;
                    ptBlocks.Add(b);
                    b = new Block();
                }
                else
                    b.bcis.Add(bci);
                start++;
            }
        }

        /// <summary>
        /// Splits blocks into 1 or more blocks based on jump distance.
        /// </summary>
        /// <param name="jumps"></param>
        /// <param name="blocks"></param>
        private void SliceBlocks(List<Jump> jumps, List<Block> blocks, Prototype currentPT)
        {
            //foreach (Block b in blocks) //debug
            //    Console.Out.WriteLine(b.ToString()); //debug
            //Console.Read(); //debug
            foreach (Jump j in jumps)
            {
                Block b = Block.GetTargetOfJump(currentPT, blocks, j.index);
                if (j.distance <= 0)
                    throw new Exception("SliceBlocks: Negative jumps unhandled currently.");
                if (b.iCount > j.distance) //we need to split the block
                {
                    Block split1 = new Block();
                    Block split2 = new Block();
                    split1.startB = b.startB;
                    int i = 0;
                    while (i < j.distance)
                    {
                        split1.bcis.Add(b.bcis[i]);
                        i++;
                    }
                    split1.endB = i - 1;
                    split2.startB = i;
                    while (i < b.iCount)
                    {
                        split2.bcis.Add(b.bcis[i]);
                        i++;
                    }
                    split2.endB = i;
                    //initialize other parts of blocks.
                    split1.currentPT = b.currentPT;
                    split2.currentPT = b.currentPT;
                    split1.ptBlocks = b.ptBlocks;
                    split2.ptBlocks = b.ptBlocks;
                    split1.varRef = b.varRef;
                    split2.varRef = b.varRef;
                    int oldIndex = blocks.IndexOf(b);
                    blocks.Remove(b);
                    blocks.Insert(oldIndex, split1);
                    blocks.Insert(oldIndex + 1, split2);
                }
                //else //debug
                //    Console.Out.WriteLine("Dist: " + j.distance + " Index: " + j.index); //debug
            }
            foreach (Block b in blocks) //debug
                Console.Out.WriteLine(b.ToString()); //debug
            Console.Read(); //debug
        }

        /// <summary>
        /// Decompiles a single block of asm.
        /// </summary>
        /// <param name="pt">Current prototype.</param>
        /// <param name="b">A block from within the current prototype.</param>
        //private void DecompileBlock(Block b, ref Variables vars)
        //{
        //    foreach(BytecodeInstruction bci in b.bcis)
        //    {
        //        switch (bci.opcode)
        //        {
        //            case OpCodes.ISLT:
        //            case OpCodes.ISGE:
        //            case OpCodes.ISLE:
        //            case OpCodes.ISGT:
        //            case OpCodes.ISEQV:
        //            case OpCodes.ISNEV:
        //            case OpCodes.ISEQS:
        //            case OpCodes.ISNES:
        //            case OpCodes.ISEQN:
        //            case OpCodes.ISNEN:
        //            case OpCodes.ISEQP:
        //            case OpCodes.ISNEP:
        //                IfSt st = new IfSt(new Expression(bci, vars));
        //                fileSource.AppendLine(st.ToString());
        //                break;
        //            case OpCodes.KSHORT:
        //                Variable v = new Variable(bci.registers[0], new CInt((bci.registers[2] << 8) | bci.registers[1]));
        //                vars.SetVar(v);
        //                //fileSource.AppendLine(vars.SetVar(v));
        //                break;
        //            default: //skip bytecode instruction as default. JMP is handled in BlockifyPT.
        //                break;
        //        }
        //    }
        //}

        public override string ToString()
        {
            return fileSource.ToString();
        }
        ////used for passing around DecPT to where it is needed.
        //public delegate string DelPT(string id, Prototype pt, ref int tabLevel);

        ///// <summary>
        ///// Decompiles an individual prototype and converts it to lua source.
        ///// </summary>
        ///// <param name="id">An ID to organize prototypes in the lua source file.</param>
        ///// <param name="pt">The prototype to decompile.</param>
        ///// <param name="tabLevel">How many tabs to indent by.</param>
        ///// <returns></returns>
        //private string DecPT(string id, Prototype pt, ref int tabLevel)
        //{
        //    StringBuilder result = new StringBuilder();
        //    Variables vars = new Variables();

        //    while (bciOffset < pt.bytecodeInstructions.Count)
        //    {
        //        BytecodeInstruction bci = pt.bytecodeInstructions[bciOffset];
        //        switch(bci.opcode)
        //        {
        //            case OpCodes.ISLT:
        //            case OpCodes.ISGE:
        //            case OpCodes.ISLE:
        //            case OpCodes.ISGT:
        //            case OpCodes.ISEQV:
        //            case OpCodes.ISNEV:
        //            case OpCodes.ISEQS:
        //            case OpCodes.ISNES:
        //            case OpCodes.ISEQN:
        //            case OpCodes.ISNEN:
        //            case OpCodes.ISEQP:
        //            case OpCodes.ISNEP:
        //                IfSt ifst = new IfSt(pt, DecPT, ref vars, ref bciOffset, ref tabLevel);
        //                result.AppendLine(ifst.ToString());
        //                break;
        //            case OpCodes.KSHORT:
        //                Variable v = new Variable(bci.registers[0], new CInt((bci.registers[1] << 8) | bci.registers[2]));
        //                result.AppendLine(vars.SetVar(v.index, v));
        //                bciOffset++;
        //                break;
        //            default: //skip bytecode instruction as default. JMP also handled already by blockifyPT.
        //                bciOffset++;
        //                break;
        //        }
        //    }
        //    bciOffset = 0; //return bcioffset to 0 after the proto is done.
        //    return result.ToString();
        //}

        /// <summary>
        /// Generates a prototype ID for naming functions: function ID( )
        /// </summary>
        /// <returns></returns>
        private string GenId(Prototype pt)
        {
            //For now we use the prototype index. if index == 0, it is the "main" prototype.
            if (pt.index == 0)
                return "main";
            else
                return "prototype_" + pt.index + "_" + pt.GetIdFromHeader();
        }
    }
}
