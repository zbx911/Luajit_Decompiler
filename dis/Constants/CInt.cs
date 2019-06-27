using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis.Constants
{
    class CInt : BaseConstant
    {
        public CInt(int value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return "Int{ " + value + " };";
        }
    }
}
