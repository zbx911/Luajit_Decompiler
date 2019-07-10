using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dis.Constants;

namespace Luajit_Decompiler.dec.Structures
{
    class IfSt
    {
        private Expression exp;

        public IfSt(Expression exp)
        {
            this.exp = exp;
        }

        public override string ToString()
        {
            StringBuilder stmt = new StringBuilder("if ");
            stmt.Append(exp.expression);
            stmt.Append(" then");
            return stmt.ToString();
        }
    }
}
