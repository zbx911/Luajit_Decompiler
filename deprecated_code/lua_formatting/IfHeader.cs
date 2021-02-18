using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.lua_formatting
{
    class IfHeader : ConditionalHeader
    {
        public IfHeader(OpCodes op, string left, string right, int indent) : base(op, left, right, indent) { }

        public override string ToString()
        {
            return "if " + base.ToString() + " then";
        }
    }
}
