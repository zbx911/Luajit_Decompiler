using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;

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

    class IfSt : BaseSt
    {
        //Mapping of opcode to source code operators for an if statement.
        private static Dictionary<OpCodes, string> map = new Dictionary<OpCodes, string>()
        {
            { OpCodes.ISLT, "<" },
            { OpCodes.ISGE, ">=" },
            { OpCodes.ISLE, "<=" },
            { OpCodes.ISGT, ">" },
            { OpCodes.ISEQV, "==" },
            { OpCodes.ISNEV, "~=" },
            { OpCodes.ISEQS, "==" },
            { OpCodes.ISNES, "~=" },
            { OpCodes.ISEQN, "==" },
            { OpCodes.ISNEN, "~=" },
            { OpCodes.ISEQP, "==" },
            { OpCodes.ISNEP, "~=" }
        };
        private string source;
        public IfSt(Prototype current, ref int bciOffset, ref int tabLevel) : base(current, ref bciOffset, ref tabLevel)
        {
            StringBuilder result = new StringBuilder();

            source = result.ToString();
        }

        public override string ToString()
        {
            return source;
        }
    }
}
