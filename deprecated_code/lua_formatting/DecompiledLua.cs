using System;
using System.Collections.Generic;
using Luajit_Decompiler.dec.data;

namespace Luajit_Decompiler.dec.lua_formatting
{
    class DecompiledLua
    {
        private List<string> decompiledLua;

        public DecompiledLua()
        {
            decompiledLua = new List<string>();
        }

        public string GetLuaSource()
        {
            return string.Join("\n", decompiledLua);
        }

        private string GetTabsByScope(int scope)
        {
            string tabs = "";
            for (int i = 0; i < scope; i++)
                tabs += "\t";
            return tabs;
        }

        public void WriteSrcFunction(string funcName, int scope)
        {

        }

        public void WriteSrcOperation(string left, string right, string op, int scope)
        {
            decompiledLua.Add(GetTabsByScope(scope) + left + " " + op + " " + right);
        }

        //if, while, for, etc...
        public void WriteSrcConstruct(object construct, int scope)
        {
            decompiledLua.Add(GetTabsByScope(scope) + construct.ToString());
        }

        public void WriteNeededEnds(Block b)
        {
            for (int scope = b.scope; scope < 0; scope--)
                WriteSrcEnd(scope);
        }

        public void WriteSrcEnd(int scope)
        {
            decompiledLua.Add(GetTabsByScope(scope) + "end");
        }

        public void WriteSrcElse(int scope)
        {
            decompiledLua.Add(GetTabsByScope(scope) + "else");
        }
    }
}
