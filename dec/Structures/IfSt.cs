using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dis.Constants;

namespace Luajit_Decompiler.dec.Structures
{
    //All are in format: OP, A, D
    //Immediately followed by a jump (JMP) instruction which is the target of the jump if true. otherwise, it goes to the instruction after the jump (JMP).

    //ISLT <
    //ISGE >=
    //ISLE <=
    //ISGT >
    //ISEQV ==
    //ISNEV !=
    //ISEQS == //var = to string ?
    //ISNES != //var != to string ?
    //ISEQN == //var == num ?
    //ISNEN != //var != num ?
    //ISEQP == //var == primitive type ?
    //ISNEP != //var != primitive type ?

        /// <summary>
        /// Note: Try grouping instructions into sets called "blocks" ending in JMP? (an idea presented by dilemming in the chat) otherwise use GOTO and ::location::
        /// </summary>
    class IfSt
    {
        //Mapping of opcode to source code operators for an if statement. Uninverted so doesn't match source and is the logical negation of the source as well.
        //private static Dictionary<OpCodes, string> map = new Dictionary<OpCodes, string>()
        //{
        //    { OpCodes.ISLT, "<" },
        //    { OpCodes.ISGE, ">=" },
        //    { OpCodes.ISLE, "<=" },
        //    { OpCodes.ISGT, ">" },
        //    { OpCodes.ISEQV, "==" },
        //    { OpCodes.ISNEV, "~=" },
        //    { OpCodes.ISEQS, "==" },
        //    { OpCodes.ISNES, "~=" },
        //    { OpCodes.ISEQN, "==" },
        //    { OpCodes.ISNEN, "~=" },
        //    { OpCodes.ISEQP, "==" },
        //    { OpCodes.ISNEP, "~=" }
        //};

        //This is a map of inverted inequality symbols. It is necessary to match source code and be logically equivalent to the source regardless of operand order. (Theoretically...)
        private static Dictionary<OpCodes, string> map = new Dictionary<OpCodes, string>()
        {
            { OpCodes.ISLT, ">=" },
            { OpCodes.ISGE, "<" },
            { OpCodes.ISLE, ">" },
            { OpCodes.ISGT, "<=" },
            { OpCodes.ISEQV, "~=" },
            { OpCodes.ISNEV, "==" },
            { OpCodes.ISEQS, "~=" },
            { OpCodes.ISNES, "==" },
            { OpCodes.ISEQN, "~=" },
            { OpCodes.ISNEN, "==" },
            { OpCodes.ISEQP, "~=" },
            { OpCodes.ISNEP, "==" }
        };
        private string startIf = "if(";
        private string startNotIf = "if not(";
        private string nextIf = ") then";
        private string source;
        private string varA; //name of a variable used for register A.
        private string varD; //for D.
        /// <summary>
        /// Prepares the value used in the ToString() method to return lua source code for an if statement.
        /// </summary>
        /// <param name="current">Current prototype we are working with.</param>
        /// <param name="decPt">The DecPT method in DecPrototypes. Used for interpreting the instructions after the jump recursively.</param>
        /// <param name="bciOffset">Current offset in the current prototype's bytecode instruction list.</param>
        /// <param name="tabLevel">Number of tab indentations.</param>
        public IfSt(Prototype current, ref Variables vars, ref int bciOffset, ref int tabLevel)
        {
            //InterpretRegisters(current, ref variables, ref bciOffset); //assigns the value/variable to the appropriate register.
            //StringBuilder exp = new StringBuilder();
            //switch(current.bytecodeInstructions[bciOffset].opcode)
            //{
            //    case OpCodes.ISLT:
            //    case OpCodes.ISLE:
            //        exp.Append(startNotIf);
            //        break;
            //    default:
            //        exp.Append(startIf);
            //        break;
            //}
            //exp.Append(varA);
            //exp.Append(" ");
            //exp.Append(varD);
            //exp.Append(" ");
            //exp.Append(nextIf);
            //bciOffset++; //increment to the JMP.
            ////Next opcode is a JMP. Read the JMP, subtract the combined D register by 0x8000 and read that many instructions minus 1. 
            ////if the next instruction is a JMP after that, it is an if/elseif/else or an if/else statement.
            //BytecodeInstruction jmp = current.bytecodeInstructions[bciOffset];
            //int jmpD = (jmp.registers[1] << 8) | jmp.registers[2];
            //bciOffset++; //next instruction after JMP.
            //tabLevel++; //instructions are inside the loop so indent.
            //for(int i = 0; i < jmpD - 1; i++)
            //{
            //    exp.AppendLine(DecPT("--If Instructions", current, ref tabLevel));
            //    bciOffset++;
            //}
            //bciOffset++;
            //tabLevel--; //fix indentation.
            ////while the next opcode after dealing with if statement JMP is not another JMP. (aka an elseif/else statement).
            
            //while(current.bytecodeInstructions[bciOffset].opcode != OpCodes.JMP)
            //{
            //    exp.Append("else" + startIf); //elseif(
            //    ///TODO: There will be at most 2 instructions before the next conditional opcode. (1 for each parameter).
            //    tabLevel++; //indent for after the else
            //    tabLevel--; //fix indent
            //}
            ////last 2 lines.
            //exp.AppendLine("end");
            //source = exp.ToString();
        }

        private void InterpretRegisters(Prototype current, ref List<Variable> variables, ref int bciOffset)
        {
            BytecodeInstruction bci = current.bytecodeInstructions[bciOffset];
            byte regAIndex = bci.registers[0];
            int regDIndex = (bci.registers[1] << 8) | bci.registers[2]; //make room for other byte by shifting 8 bits to the left and OR for register D index.
            string varA = variables[regAIndex].varName;
            string varD = variables[regDIndex].varName;
        }

        //public override string ToString()
        //{
        //    return source;
        //}
    }
}
