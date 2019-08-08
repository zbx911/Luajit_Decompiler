using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dec.Structures.gir
{
    interface INode
    {
        int Type { get; }
        INode[] GetNodes();
    }
}
