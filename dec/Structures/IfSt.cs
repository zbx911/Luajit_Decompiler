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
        private Prototype currentPT; //reference to current prototype
        private List<Block> ptBlocks; //reference to all proto blocks.
        private int nestLevel = 0;

        public IfSt(Expression exp, Prototype currentPT, List<Block> ptBlocks)
        {
            this.exp = exp;
            this.currentPT = currentPT;
            this.ptBlocks = ptBlocks;
            //get block associated with jump for this statement
            //call DecompileBlock on the block (and set written to true on the block)?
            //ensure proper indentation of nested block.
        }

        public override string ToString()
        {
            StringBuilder stmt = new StringBuilder("if ");
            stmt.Append(exp.expression);
            stmt.AppendLine(" then");
            //find target of jump, indent and write the block.
            Block jmpTarget = Block.GetTargetOfJump(currentPT, ptBlocks, exp.condi.index + 1); //1 more than the index of the conditional is the jump location.
            nestLevel++;
            stmt.AppendLine(Block.NestSource(jmpTarget.WriteBlock(jmpTarget.varRef), ref nestLevel));
            nestLevel--;
            stmt.AppendLine("end");
            return stmt.ToString();
        }
    }
}
