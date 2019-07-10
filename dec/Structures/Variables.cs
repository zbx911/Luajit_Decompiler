using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis.Constants;

namespace Luajit_Decompiler.dec.Structures
{
    /// <summary>
    /// Used to manage the variables of a prototype.
    /// </summary>
    class Variables
    {
        public int count;
        public Variable[] vs;

        public Variables()
        {
            count = 0;
            vs = new Variable[10];
        }

        public string SetVar(Variable var)
        {
            string result;
            if (var.index > vs.Length - 1) //if array is too small.
                System.Array.Resize(ref vs, vs.Length * 2);
            if (vs[var.index] != null) //value present at index.
            {
                result = VarSetSource(var);
                //result = "";
            }
            else
                result = NewVarSource(var);
            vs[var.index] = var;
            return result;
        }

        private string VarSetSource(Variable var)
        {
            return var.varName + " = " + var.value.GetValue();
        }

        private string NewVarSource(Variable var)
        {
            return "local " + var.varName + " = " + var.value.GetValue();
        }
    }
    class Variable
    {
        public string varName;
        public BaseConstant value;
        public int index;

        public Variable(int index, BaseConstant value)
        {
            this.value = value;
            this.index = index;
            varName = "var" + index;
        }
    }
}
