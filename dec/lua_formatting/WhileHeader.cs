using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.lua_formatting
{
    class WhileHeader : ConditionalHeader
    {
        public WhileHeader(OpCodes op, string left, string right, int indent) : base(op, left, right, indent) { }

        public override string ToString()
        {
            return "while " + base.ToString() + "do";
        }
    }
}
