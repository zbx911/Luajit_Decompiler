using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis.Constants
{
    class BaseConstant
    {
        protected object value;

        public virtual object GetValue()
        {
            return value;
        }
    }
}
