using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis.Constants
{
    class LuaNumber
    {
        public int low { get; }
        public int high { get; }
        public LuaNumber(int low, int high)
        {
            this.low = low;
            this.high = high;
        }
        
        /// <summary>
        /// Returns lua number in format -> low:high
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LuaNum{ " + low + ":" + high + " };";
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
