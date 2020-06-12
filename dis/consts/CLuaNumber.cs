namespace Luajit_Decompiler.dis.consts
{
    class LuaNumber
    {
        public int Low { get; }
        public int High { get; }
        public LuaNumber(int low, int high)
        {
            this.Low = low;
            this.High = high;
        }
        
        /// <summary>
        /// Returns lua number in format -> low:high
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LuaNum{ " + Low + ":" + High + " };";
        }
    }

    class CLuaNumber : BaseConstant
    {
        public CLuaNumber(LuaNumber value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
