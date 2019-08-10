using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dec.gir
{
    interface INode
    {
        int Type { get; } //type of node. 0 = code, 1 = conditional.
        INode Next { get; set; } //main code path of the graph.
    }
}
