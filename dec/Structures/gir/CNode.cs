using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dec.Structures.gir
{
    class CNode : INode
    {
        public int Type { get { return 0; } }
        INode next;

        public INode[] GetNodes()
        {
            return new INode[] { next };
        }
    }
}
