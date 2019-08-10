using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis.Constants;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec;
using Luajit_Decompiler.dec.gir;
using Luajit_Decompiler.dec.Structures;

namespace Luajit_Decompiler.dec.gir
{
    class PtGraph
    {
        public static BaseConstant[] variables; //might be necessary for expressions.
        private List<BytecodeInstruction> bcis;
        private List<Jump> jumps;
        public CNode root;

        public PtGraph()
        {
            variables = new BaseConstant[2];
            bcis = DecPrototypes.pt.bytecodeInstructions;
            jumps = DecPrototypes.jumps;
            GraphPrototype();
        }

        private void GraphPrototype()
        {
            //pass #2 of bytecode
            //go through until we hit isge/jmp
            //if we hit one, finish the first code node, create new if node with expression, link code node to if node, continue.

            //handle first jump at top of file.
            Block top = jumps[0].target;
            if (jumps.Count > 1)
                top.Finalize(jumps[1].index);
            else
                top.Finalize(bcis.Count);
            root = new CNode(top);

            //for(int i = 1; i < jumps.Count; i++)
            //{
            //    if (jumps[i].jumpType == 1) //regular jump
            //    {
            //        AddNode(new CNode(jumps[i].target)); //if false OR rest of code.
            //    }
            //    else //conditional jump
            //    {
            //        IfNode condi = new IfNode(new Expression(bcis[i].opcode, variables[bcis[i].registers[0]].ToString(), variables[bcis[i].registers[1]].ToString()));
            //        AddNode(condi);
            //        AddNode(new CNode(jumps[i].target)); //if true
            //    }
            //}
        }

        private Block FindJumpTarget(int jndx)
        {
            foreach (Jump j in jumps)
                if (j.index == jndx)
                    return j.target;
            throw new Exception("Target not found.");
        }

        /// <summary>
        /// Inserts into the next available node slot. (Order: True then False for condi nodes)
        /// </summary>
        /// <param name="node"></param>
        private void AddNode(INode node)
        {

        }

        /// <summary>
        /// Do a breadth first search through the graph. append source as you go. Return when a node has no children.
        /// </summary>
        /// <returns></returns>
        private void BreadthFirstPrint()
        {
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();

            res.AppendLine("local temp1, temp2");

            return res.ToString();
        }
    }
}
