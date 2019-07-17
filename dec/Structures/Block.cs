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
    /// <summary>
    /// Consider having each block write itself.
    /// </summary>
    class Block
    {
        public int startB; //start of the block. (Relative to the asm lines).
        public int endB; //end of the block.
        public List<BytecodeInstruction> bcis; //all the bytecode instructions in a block.
        public List<Block> ptBlocks; //reference to the list of all PT blocks.
        public Prototype currentPT; //reference to current prototype.
        public bool written; //has the block already been written to source?
        public int iCount { get { return bcis.Count; } } //instruction count.
        public Variables varRef;
        //public int nextJumpIndex { get { return endB + 1; } } //location of the jump after this block. can be null in event of RET0 causing index out of bounds error. (bci.count = ret0 location).

        public Block()
        {
            written = false;
            bcis = new List<BytecodeInstruction>();
        }

        public string WriteBlock(Variables vars)
        {
            StringBuilder source = new StringBuilder();
            foreach (BytecodeInstruction bci in bcis)
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
                        IfSt st = new IfSt(new Expression(bci, vars), currentPT, ptBlocks);
                        source.AppendLine(st.ToString());
                        break;
                    case OpCodes.KSHORT:
                        Variable v = new Variable(bci.registers[0], new CInt((bci.registers[2] << 8) | bci.registers[1]));
                        vars.SetVar(v);
                        //source.AppendLine(vars.SetVar(v));
                        break;
                    default: //skip bytecode instruction as default. JMP/RET is handled in BlockifyPT.
                        break;
                }
            }
            written = true;
            return source.ToString();
        }

        /// <summary>
        /// Properly indents source code that should be nested. For example, if statements and loops.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string NestSource(string source, ref int nestLevel)
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

        /// <summary>
        /// Returns the block associated with a jump.
        /// </summary>
        /// <param name="pt">Current prototype.</param>
        /// <param name="jumpIndex">Location of the jump opcode in the asm.</param>
        /// <returns></returns>
        public static Block GetTargetOfJump(Prototype pt, List<Block> ptBlocks, int jumpIndex)
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
                    if (b.startB == jumpIndex) //if statements are usually positive jumps in which the next instruction after the JMP instruction is the start of its block.
                        return b;
            throw new Exception("Block not found.");
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder("---Block---\n");
            foreach (BytecodeInstruction bci in bcis)
                res.AppendLine(bci.ToString());
            res.AppendLine("---End Block---");
            return res.ToString();
            //return WriteBlock(varRef);
        }
    }
}
