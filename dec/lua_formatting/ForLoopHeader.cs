namespace Luajit_Decompiler.dec.lua_formatting
{
    class ForLoopHeader : LuaConstruct
    {
        private int start;
        private int stop;
        private int inc;

        public ForLoopHeader(int start, int stop, int inc, int indent) : base(indent)
        {
            this.start = start;
            this.stop = stop;
            this.inc = inc;
        }

        public override string ToString()
        {
            return GetIndentationString() + "for " + start + ", " + stop + ", " + inc + " do";
        }
    }
}
