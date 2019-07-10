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

        /// <summary>
        /// Decompiles an entire file's prototypes.
        /// </summary>
        /// <param name="name">Name of the entire file.</param>
        /// <param name="pts">List containing that file's prototypes.</param>
        public DecPrototypes(string name, List<Prototype> pts)
        {
            StringBuilder res = new StringBuilder();
            res.AppendLine("--Lua File Name: " + name);
            Variables vars = new Variables(); //temp vars for the file.
            for (int i = pts.Count; i > 0; i--) //We go backwards here because the 'main' proto is always the last one and will have the most prototype children.
            {
                int blockifyIndex = 0;
                ptBlocks = new List<Block>();
                BlockifyPT(pts[i - 1], ref blockifyIndex, pts[i - 1].bytecodeInstructions.Count); //Note: After each block's endIndex is a JMP in the prototype asm. (endIndex + 1)
                foreach(Block b in ptBlocks)
                {
                    DecompileBlock(b, ref vars);
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
                switch (bci.opcode)
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
                    b.endB = start - 1; //terminate block at instruction above JMP.
                    ptBlocks.Add(b);
                    b = new Block();
                }
                else
                    b.bcis.Add(bci);
                start++;
            }
        }

        /// <summary>
        /// Decompiles a single block of asm.
        /// </summary>
        /// <param name="pt">Current prototype.</param>
        /// <param name="b">A block from within the current prototype.</param>
        private void DecompileBlock(Block b, ref Variables vars)
        {
            foreach(BytecodeInstruction bci in b.bcis)
            {
                switch (bci.opcode)
                {
                    case OpCodes.ISLT:
                    case OpCodes.ISGE:
                    case OpCodes.ISLE:
                    case OpCodes.ISGT:
                    case OpCodes.ISEQV:
                    case OpCodes.ISNEV:
                    case OpCodes.ISEQS:
                    case OpCodes.ISNES:
                    case OpCodes.ISEQN:
                    case OpCodes.ISNEN:
                    case OpCodes.ISEQP:
                    case OpCodes.ISNEP:
                        IfSt st = new IfSt(new Expression(bci, vars));
                        fileSource.AppendLine(st.ToString());
                        break;
                    case OpCodes.KSHORT:
                        Variable v = new Variable(bci.registers[0], new CInt((bci.registers[2] << 8) | bci.registers[1]));
                        vars.SetVar(v);
                        //fileSource.AppendLine(vars.SetVar(v));
                        break;
                    default: //skip bytecode instruction as default. JMP is handled in BlockifyPT.
                        break;
                }
            }
        }

        /// <summary>
        /// Returns the block associated with a jump.
        /// </summary>
        /// <param name="pt">Current prototype.</param>
        /// <param name="jumpIndex">Location of the jump opcode in the asm.</param>
        /// <returns></returns>
        private Block GetTargetOfJump(Prototype pt, int jumpIndex)
        {
            ///TODO: Implement negative jumps.
            BytecodeInstruction jump = pt.bytecodeInstructions[jumpIndex];
            if (jump.opcode != OpCodes.JMP)
                throw new Exception("Given jump index is not correct. The instruction at the index is: " + jump.opcode);
            int target = ((jump.registers[2] << 8) | jump.registers[1]) - 0x8000; //can be negative
            if (target <= 0)
                throw new Exception("Negative jump targets unimplemented.");
            else
                foreach (Block b in ptBlocks) //negative blocks will have to search for the start of the block probably. Positive jumps need to search for end of a block.
                    if (b.startB == jumpIndex + 1) //if statements are usually positive jumps in which the next instruction after the JMP instruction is the start of its block.
                        return b;
            throw new Exception("Block not found.");
        }

        /// <summary>
        /// Properly indents source code that should be nested. For example, if statements and loops.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private string NestSource(string source, ref int nestLevel)
        {
            StringBuilder tabs = new StringBuilder();
            StringBuilder formattedSource = new StringBuilder();
            for (int i = 0; i < nestLevel; i++)
                tabs.Append("\t");
            string[] lines = source.Split('\n');
            foreach (string line in lines)
                formattedSource.AppendLine(tabs.ToString() + line);
            return formattedSource.ToString();
        }

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
