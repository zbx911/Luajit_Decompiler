using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dec.Structures.gir
{
    class IfNode : INode
    {
        public int Type { get { return 1; } }
        INode nTrue;
        INode nFalse;

        public INode[] GetNodes()
        {
            return new INode[] { nTrue, nFalse };
        }
    }
}
