using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dec.Structures;

namespace Luajit_Decompiler.dec.gir
{
    class IfNode : INode
    {
        public int Type { get { return 1; } }
        public INode Next { get; set; }
        public Expression exp;
        public INode nTrue; //code that executes if exp is true.

        /// <summary>
        /// Creates a conditional node given the expression that points to other nodes.
        /// </summary>
        /// <param name="exp">Expression of the comparison opcode.</param>
        public IfNode(Expression exp)
        {
        }
    }
}
