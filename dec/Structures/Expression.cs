using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.Structures
{
    /// <summary>
    /// The purposes of this class is to take conditional opcodes and translate them into boolean expressions. Options for returning an expression for if statements/loops are available.
    /// </summary>
    class Expression
    {
        /// TODO: Implement returning if statement/loop statement shells.

        //This is a map of inverted inequality symbols. It is necessary to match source code and be logically equivalent to the source regardless of operand order. (Theoretically...)
        private static Dictionary<OpCodes, string> map = new Dictionary<OpCodes, string>()
        {
            { OpCodes.ISLT, "<" }, //part of a negated expression. { if not (expression) for example. }
            { OpCodes.ISGE, "<" },
            { OpCodes.ISLE, "<=" }, //part of a negated expression. { if not (expression) for example. }
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

        public string expression;

        /// <summary>
        /// Constructs an expression.
        /// </summary>
        /// <param name="condi">Conditional instruction.</param>
        /// <param name="vars">Current temporary variables. (From KSHORT instructions for example).</param>
        public Expression(BytecodeInstruction condi, Variables vars)
        {
            bool isCondi = false;
            foreach (KeyValuePair<OpCodes, string> k in map)
                if (condi.opcode == k.Key)
                {
                    isCondi = true;
                    break;
                }
            if (!isCondi)
                throw new Exception("Given opcode is not a conditional instruction. Opcode is: " + condi.opcode);

            byte regAIndex = condi.registers[0];
            int regDIndex = (condi.registers[2] << 8) | condi.registers[1];
            bool isNotExpression = false; //if it is a NOT expression. see map comments about negated expressions.
            switch (condi.opcode)
            {
                case OpCodes.ISLT:
                case OpCodes.ISLE:
                    isNotExpression = true;
                    break;
                default:
                    break;
            }
            StringBuilder result = new StringBuilder();
            if (isNotExpression)
                result.Append("not ");
            result.Append("(" + vars.vs[regAIndex].value.GetValue() + " " + map[condi.opcode] + " " + vars.vs[regDIndex].value.GetValue() + ")");
            expression = result.ToString();
        }
    }

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
}
