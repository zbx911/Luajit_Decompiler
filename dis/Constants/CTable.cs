using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis.Constants
{
    class CTable : BaseConstant
    {
        public CTable(TableConstant value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
