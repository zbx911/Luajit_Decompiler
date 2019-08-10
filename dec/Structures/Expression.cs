using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.gir;

namespace Luajit_Decompiler.dec.Structures
{
    class Expression
    {
        //This is a map of inverted inequality symbols. It is necessary to match source code and be logically equivalent to the source regardless of operand order. (Theoretically...)
        private static readonly Dictionary<OpCodes, string> map = new Dictionary<OpCodes, string>()
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
        public Expression(OpCodes op, string left, string right)
        {
            expression = "(" + left + " " + map[op] + " " + right + ")";
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
