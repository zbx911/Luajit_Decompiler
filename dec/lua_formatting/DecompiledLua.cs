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

        public void AddLine(string line)
        {
            decompiledLua.Add(line);
        }

        public void AddLuaConstructHeader(LuaConstruct header)
        {
            decompiledLua.Add(header.ToString());
        }

        public override string ToString()
        {
            return string.Join("\n", decompiledLua);
        }

        public void AddEnd()
        {
            AddLine(GetIndentationString() + "end");
        }

        public void AddElseClause()
        {
            AddLine(GetIndentationString() + "else");
        }
    }
}
