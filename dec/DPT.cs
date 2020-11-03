using System.Collections.Generic;
using System.Linq;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.gir;
using Luajit_Decompiler.dec.tluastates;
using System.Text;

namespace Luajit_Decompiler.dec
{
    class DPT //Decompile Prototype
    {
        private Prototype pt;
        private readonly List<BytecodeInstruction> ptBcis;
        private List<Jump> jumps;
        private List<Block> blocks;
        private List<string> decompLines; //Prototype decompilation buffer.

        public DPT(Prototype pt)
        {
            this.pt = pt;
            ptBcis = pt.bytecodeInstructions;
            jumps = new List<Jump>();
            blocks = new List<Block>();
            BeginDecompile();
        }

        /// <summary>
        /// Correctly orders certain method calls from 1 method call.
        /// </summary>
        private void BeginDecompile()
        {
            //Chop a prototype into managable blocks based on jumps.
            BlockPrototype();

            //create control flow graph using adjacency matrix
            Cfg cfg = new Cfg(ref jumps, ref blocks);

            decompLines = new List<string>();

            //TODO: Iterate over each block minding the CFG and attempt to generate lua source.
            TLuaState tls = new TLuaState(ref pt, ref cfg, ref blocks, ref decompLines);

            //begin decompilation state machine.
            new BeginState(tls);
        }

        /// <summary>
        /// Gets jump targets and finalizes those blocks.
        /// Refactored based on DiLemming's suggestion.
        /// </summary>
        private void BlockPrototype()
        {
            List<BytecodeInstruction> ptBcis = pt.bytecodeInstructions;

            //make a jmp bci to top of file
            BytecodeInstruction jmpTop = new BytecodeInstruction(OpCodes.JMP, -1);
            jmpTop.regs.regA = 0;
            jmpTop.regs.regC = 0;
            jmpTop.regs.regB = 128;
            Jump top = new Jump(jmpTop, 1, -1, pt);
            jumps.Add(top);

            int name = 0;

            //get jump targets
            for (int i = 0; i < ptBcis.Count; i++) //O(N)
            {
                int check = CheckCJR(ptBcis[i]);
                if (check == 1 || check == 3) //jmp or comparison
                {
                    Jump j = new Jump(ptBcis[i], check, name, pt);
                    jumps.Add(j);
                    name++;
                }
            }

            SortedSet<int> targets = new SortedSet<int>(new ByDescending());

            foreach(Jump j in jumps)
                targets.Add(j.target);

            name = 0;
            
            for (int i = 0; i < targets.Count; i++) //i = start of block, i+1 = end of block.
            {
                Block b = new Block(targets.ElementAt(i), name, pt);

                if (i+1 >= targets.Count)
                {
                    b.Finalize(ptBcis.Count);
                    blocks.Add(b);
                }
                else
                {
                    b.Finalize(targets.ElementAt(i+1));
                    blocks.Add(b);
                }
                name++;
            }

            //set jumps to their targeted block
            foreach(Jump j in jumps) // ~ O(N^2) :(
            {
                foreach(Block b in blocks)
                {
                    if(j.target == b.sIndex)
                    {
                        j.TargetedBlock = b;
                        break;
                    }
                }
            }

            #region debugging BlockPrototype
            FileManager.ClearDebug();
            FileManager.WriteDebug("Bci total: " + pt.bytecodeInstructions.Count + " From Index: 0-" + (pt.bytecodeInstructions.Count - 1));
            //FileManager.WriteDebug("TEST: " + jumps[0].Block.label + " :: " + blocks[0].label);
            //blocks[0].label = "banana";
            //FileManager.WriteDebug("TEST: " + jumps[0].Block.label + " :: " + blocks[0].label); //confirmed by reference.
            foreach (Jump j in jumps)
                FileManager.WriteDebug("Jump Index: " + j.index + " -> " + j.TargetedBlock.label);
            FileManager.WriteDebug("\r\n");
            FileManager.WriteDebug("------------------------------");
            FileManager.WriteDebug("\r\n");
            foreach (Block b in blocks)
                FileManager.WriteDebug(b.ToString());
            #endregion
        }

        /// <summary>
        /// Check for Condi, Jump, or Ret opcodes.
        /// </summary>
        /// <param name="bci"></param>
        /// <returns></returns>
        private int CheckCJR(BytecodeInstruction bci)
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
    }

    internal class ByDescending : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x < y)
                return -1;
            if (x > y)
                return 1;
            else //equal
                return 0;
        }
    }
}
