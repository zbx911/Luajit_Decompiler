using System.Text;

namespace Luajit_Decompiler.dec.lua_formatting
{
    class LuaConstruct
    {
        public int indent;

        public LuaConstruct(int indent)
        {
            this.indent = indent;
        }

        protected string GetIndentationString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < indent; i++)
                sb.Append("\t");
            return sb.ToString();
        }
    }
}
