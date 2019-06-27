using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis.Constants
{
    class CNil : BaseConstant
    {
        public CNil()
        {
            value = "nil";
        }

        public override string ToString()
        {
            return "Nil{ };";
        }
    }
}
