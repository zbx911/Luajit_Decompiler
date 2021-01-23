using System.Collections.Generic;

namespace Luajit_Decompiler.dec.lua_formatting
{
    class DecompiledLua : LuaConstruct
    {
        private List<string> decompiledLua;

        public DecompiledLua(int indent) : base(indent)
        {
            decompiledLua = new List<string>();
        }

        private void AddLine(string line)
        {
            decompiledLua.Add(line);
        }

        public void AddLuaConstructHeader(LuaConstruct header)
        {
            decompiledLua.Add(header.ToString());
        }

        public override string ToString()
        {
            return string.Join("\n", decompiledLua); //i don't think this is doing indentations properly
        }

        public void AddEnd()
        {
            AddLine(GetIndentationString() + "end");
        }

        public void AddElseClause()
        {
            AddLine(GetIndentationString() + "else");
        }

        public void AddAssignment(string left, string right)
        {
            AddLine(GetIndentationString() + left + " = " + right);
        }
    }
}
