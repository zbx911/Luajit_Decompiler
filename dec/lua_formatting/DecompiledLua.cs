using Luajit_Decompiler.dec.state_machine;
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

        public void CheckAddAssignmentAndSetAccessed((string, bool)dstAndLeft, string right, LStateContext ctx)
        {
            if (!dstAndLeft.Item2)
            {
                AddLocalAssignment(dstAndLeft.Item1, right);
                ctx.varNames.SetAccessed(dstAndLeft.Item1);
            }
            else
                AddAssignment(dstAndLeft.Item1, right);
        }

        private void AddAssignment(string left, string right)
        {
            AddLine(GetIndentationString() + left + " = " + right);
        }

        private void AddLocalAssignment(string left, string right)
        {
            AddLine(GetIndentationString() + "local " + left + " = " + right);
        }
    }
}
