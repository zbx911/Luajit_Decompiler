using Luajit_Decompiler.dis;
using System.Collections.Generic;

namespace Luajit_Decompiler.dec.data
{
    class Condition
    {
        //Inverted to fix LuaJit's mapping
        protected Dictionary<OpCodes, string> map = new Dictionary<OpCodes, string>()
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

        private string a;
        private string b;
        private string op;

        /// <summary>
        /// (A op B)
        /// Example: (A == 2)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="op"></param>
        public Condition(Prototype pt, BytecodeInstruction condi)
        {
            //this.a = a;
            //this.b = b;
            op = map[condi.opcode];
        }

        public override string ToString()
        {
            return "(" + a + " " + op + " " + b + ")";
        }
    }
}
