using System.Collections.Generic;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.lua_formatting
{
    class ConditionalHeader : LuaConstruct
    {
        //Inverted
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

        protected string left;
        protected string right;
        protected string op;

        public ConditionalHeader(OpCodes op, string left, string right, int indent) : base(indent)
        {
            this.left = left;
            this.right = right;
            this.op = map[op];
        }

        public override string ToString()
        {
            return left + " " + op + " " + right;
        }
    }
}
