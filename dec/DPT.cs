using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;
using Luajit_Decompiler.dec.lir;

namespace Luajit_Decompiler.dec
{
    /// <summary>
    /// TODO: Bugfix on line 75.
    /// </summary>
    class DPT //Decompile Prototype
    {
        private Prototype pt;
        private List<BytecodeInstruction> ptBcis;
        public List<Jump> jumps; //public for now for debug purposes.
        private List<BytecodeInstruction> comparisonBCIs;
        private List<Block> blocks;

        public DPT(Prototype pt)
        {
            this.pt = pt;
            ptBcis = pt.bytecodeInstructions;
            jumps = new List<Jump>();
            comparisonBCIs = new List<BytecodeInstruction>();
            blocks = new List<Block>();
            CreateIR();
        }

        /// <summary>
        /// Correctly orders certain method calls from 1 method call.
        /// </summary>
        private void CreateIR()
        {
            #region debugging map
            //IRIMap ir = new IRIMap();
            //FileManager.ClearDebug();
            //var ops = Enum.GetValues(typeof(OpCodes));
            //foreach(OpCodes op in ops)
            //    FileManager.WriteDebug(ir.Translate(op).ToString());
            #endregion

            BlockPrototype(); //#1
            //remove duplicate block instructions
            //create control flow graph using adjacency matrix
            //simplify graph if possible.
            //translate block instructions into the first IR
        }

        /// <summary>
        /// Gets jump targets and finalizes those blocks.
        /// </summary>
        private void BlockPrototype()
        {
            List<BytecodeInstruction> ptBcis = pt.bytecodeInstructions;

            //find condi and jump treat them as jumps. pass #1 of bytecode
            int name = 0;

            //make a jmp bci to top of file
            BytecodeInstruction jmpTop = new BytecodeInstruction(OpCodes.JMP, -1);
            jmpTop.AddRegister(0);
            jmpTop.AddRegister(0);
            jmpTop.AddRegister(128);
            Jump top = new Jump(jmpTop, 1, -1, pt);
            jumps.Add(top);

            //get jump targets
            for (int i = 0; i < ptBcis.Count; i++)
            {
                int check = CheckCJR(ptBcis[i]);
                if (check == 1 || check == 3) //jmp or comparison
                {
                    Jump j = new Jump(ptBcis[i], check, name, pt);
                    jumps.Add(j);
                    name++;
                }
            }

            //Block = (J).target to (J+1).target - 1
            Stack<int> skips = new Stack<int>(); //indicies for jumps that skip more than 1 block or are looping jumps.
            for (int i = 0; i < jumps.Count; i++)
            {
                Block b;
                if(jumps[i].index > jumps[i].target) //points to a block that should already exist.
                {
                    //b = FindBlockByIndexRange(jumps[i].target);
                    //jumps[i].Block = b;
                    skips.Push(i);
                }
                else if (i + 1 >= jumps.Count) //is the last block in the list and does not point back to an existing block.
                {
                    b = new Block(jumps[i].target, i, pt);
                    b.Finalize(ptBcis.Count);
                    blocks.Add(b);
                    jumps[i].Block = b;
                }
                else if(i + 1 < jumps.Count) //there exists a block after this one.
                {
                    if (jumps[i].target > jumps[i + 1].target - 1) //skips ahead more than 1 block. push index to stack and deal with it later. another jump *should* jump to it.
                        skips.Push(i);
                    else //normal block to add where J.target -> j+1.target - 1
                    {
                        b = new Block(jumps[i].target, i, pt);
                        b.Finalize(jumps[i + 1].target);
                        blocks.Add(b);
                        jumps[i].Block = b;
                    }
                }
                else
                {
                    throw new Exception("BlockPrototype: Encountered something unexpected.");
                }
            }

            //remove duplicate information
            for(int i = 0; i < blocks.Count; i++)
            {
                Block b1 = blocks[i];

                for (int j = i + 1; j < blocks.Count; j++)
                {
                    if (j >= blocks.Count)
                        break;

                    Block b2 = blocks[j];
                    int exists = b1.IndexExists(b2.sIndex);
                    if (exists != -1)
                        b1.RemoveDuplicateInfo(b2.sIndex, b2.eIndex, exists);
                }
            }

            //handle pushed blocks.
            while(skips.Count > 0)
            {
                int i = skips.Pop();
                jumps[i].Block = FindBlockByIndexRange(jumps[i].target); //these long jumps can typically can hit a RET statement and be null.
                if (jumps[i].Block == null)
                {
                    if(jumps[i].target == ptBcis.Count - 1 && CheckCJR(ptBcis[jumps[i].target]) == 2)
                    {
                        //it's a return statement so lets make a block for the return and assign it to the jump.
                        Block ret = new Block(jumps[i].target, i, pt);
                        ret.Finalize(jumps[i].target + 1);
                        jumps[i].Block = ret;
                        blocks.Add(ret);
                    }
                    else
                    {
                        throw new Exception("BlockPrototype:2ndPass::Unexpected occurrence.");
                    }
                }
            }
            //fix block labels in an unclean way :(
            for (int i = 0; i < blocks.Count; i++)
                blocks[i].label = "Block[" + i + "]";

            FileManager.ClearDebug();
            FileManager.WriteDebug("Bci total: " + pt.bytecodeInstructions.Count + " From Index: 0-" + (pt.bytecodeInstructions.Count - 1));
            //FileManager.WriteDebug("TEST: " + jumps[0].Block.label + " :: " + blocks[0].label);
            //blocks[0].label = "banana";
            //FileManager.WriteDebug("TEST: " + jumps[0].Block.label + " :: " + blocks[0].label); //confirmed by reference.
            foreach (Jump j in jumps)
                FileManager.WriteDebug("Jump Index: " + j.index + " -> " + j.Block.label);
            FileManager.WriteDebug("\r\n");
            FileManager.WriteDebug("------------------------------");
            FileManager.WriteDebug("\r\n");
            foreach (Block b in blocks)
                FileManager.WriteDebug(b.ToString());
        }

        /// <summary>
        /// Check for Condi, Jump, or Ret opcodes.
        /// </summary>
        /// <param name="bci"></param>
        /// <returns></returns>
        public static int CheckCJR(BytecodeInstruction bci)
        {
            switch (bci.opcode)
            {
                case OpCodes.JMP:
                    return 1; //jump
                case OpCodes.RET:
                case OpCodes.RET0:
                case OpCodes.RET1:
                case OpCodes.RETM:
                    return 2; //return
                case OpCodes.ISEQN:
                case OpCodes.ISEQP:
                case OpCodes.ISEQS:
                case OpCodes.ISEQV:
                case OpCodes.ISGE:
                case OpCodes.ISGT:
                case OpCodes.ISLE:
                case OpCodes.ISLT:
                case OpCodes.ISNEN:
                case OpCodes.ISNEP:
                case OpCodes.ISNES:
                case OpCodes.ISNEV:

                case OpCodes.ISF: //unary test/copy ops. (Evaluates register D in a boolean context).
                case OpCodes.IST:
                case OpCodes.ISFC:
                case OpCodes.ISTC:
                    return 3; //conditional
                default:
                    return -1; //not condi/jmp/ret
            }
        }

        private Jump FindJumpAtIndex(int index)
        {
            for (int i = 0; i < jumps.Count; i++)
                if (jumps[i].index == index)
                    return jumps[i];
            throw new Exception("Jump not found.");
        }

        private Block FindBlockByIndexRange(int index) //57
        {
            foreach(Block b in blocks)
                foreach(BytecodeInstruction bci in b.bcis)
                    if (bci.index == index)
                        return b;
            return null;
        }

        public override string ToString()
        {
            return ""; //return prototype source here.
        }
    }
}
