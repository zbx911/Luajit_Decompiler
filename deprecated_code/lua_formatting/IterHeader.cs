using Luajit_Decompiler.dis;
using System;

namespace Luajit_Decompiler.dec.lua_formatting
{
    class IterHeader : ConditionalHeader
    {
        public IterHeader(OpCodes op, string left, string right, int indent) : base(op, left, right, indent) { }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
