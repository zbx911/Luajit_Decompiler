using System.Collections.Generic;
using System.Linq;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.state_machine;
using Luajit_Decompiler.dec.lua_formatting;

namespace Luajit_Decompiler.dec
{
    class DecompilePrototype
    {
        private Prototype pt;
        private readonly List<BytecodeInstruction> ptBcis;
        private List<Jump> jumps;
        private List<Block> blocks;
        private DecompiledLua lua;

        public DecompilePrototype(Prototype pt, DecompiledLua lua)
        {
            this.pt = pt;
            this.lua = lua;
            ptBcis = pt.bytecodeInstructions;
            jumps = new List<Jump>();
            blocks = new List<Block>();
        }

        public void StartProtoDecomp()
        {
            BlockPrototype();
            BlockWriter blockw = new BlockWriter(pt, new ControlFlowGraph(jumps, blocks), lua);
            blockw.WriteBlocks();

            DebugDecompLua();
        }

        private void BlockPrototype()
        {
            AddTopOfFileJump();
            FindAllJumps();
            FinalizeBlocks(GetJumpTargets());
            SetBlockTargets();
            DebugBlockPrototype();
        }

        private void DebugDecompLua()
        {
            FileManager.ClearDebug();
            FileManager.WriteDebug(lua.ToString());
        }

        private void DebugBlockPrototype()
        {
            FileManager.ClearDebug();
            FileManager.WriteDebug("Bci total: " + pt.bytecodeInstructions.Count + " From Index: 0-" + (pt.bytecodeInstructions.Count - 1) + "\n\n");
            foreach (Jump j in jumps)
                FileManager.WriteDebug("Jump Index: " + j.index + " -> " + j.TargetedBlock.label + "\n");
            FileManager.WriteDebug("\n");
            FileManager.WriteDebug("------------------------------");
            FileManager.WriteDebug("\n");
            foreach (Block b in blocks)
                FileManager.WriteDebug(b.ToString() + "\n");
        }

        private void SetBlockTargets()
        {
            foreach (Jump j in jumps)
            {
                foreach (Block b in blocks)
                {
                    if (j.target == b.startIndex)
                    {
                        j.TargetedBlock = b;
                        break;
                    }
                }
            }
        }

        private void FinalizeBlocks(SortedSet<int> targets)
        {
            int name = 0;
            for (int i = 0; i < targets.Count; i++) //i = start of block, i+1 = end of block.
            {
                Block b = new Block(targets.ElementAt(i), name, pt);

                if (i + 1 >= targets.Count)
                {
                    b.Finalize(ptBcis.Count);
                    blocks.Add(b);
                }
                else
                {
                    b.Finalize(targets.ElementAt(i + 1));
                    blocks.Add(b);
                }
                name++;
            }
        }

        private void FindAllJumps()
        {
            int name = 0;
            for (int i = 0; i < ptBcis.Count; i++)
            {
                int check = CheckJumpOrRet(ptBcis[i]);
                if (check > 0) //is jump or comparison/loop jump.
                {
                    Jump j = new Jump(ptBcis[i], check, name, pt);
                    jumps.Add(j);
                    name++;
                }
            }
        }

        private SortedSet<int> GetJumpTargets()
        {
            SortedSet<int> targets = new SortedSet<int>(new ByDescending());

            foreach (Jump j in jumps)
                targets.Add(j.target);

            return targets;
        }

        private void AddTopOfFileJump()
        {
            BytecodeInstruction jmpTop = new BytecodeInstruction(OpCodes.JMP, -1);
            jmpTop.registers.a = 0;
            jmpTop.registers.c = 0;
            jmpTop.registers.b = 128;
            Jump top = new Jump(jmpTop, 1, -1, pt);
            jumps.Add(top);
        }

        /// <summary>
        /// Checks a bytecode instruction's opcode to see if it has a jump target or is a return.
        /// Returns >1 if it is has a jump.
        /// Returns 0 for return.
        /// Returns -1 if it has no jump.
        /// </summary>
        /// <param name="bci"></param>
        /// <returns></returns>
        private int CheckJumpOrRet(BytecodeInstruction bci)
        {
            switch (bci.opcode)
            {
                case OpCodes.RET:
                case OpCodes.RET0:
                case OpCodes.RET1:
                case OpCodes.RETM:
                    return 0;

                case OpCodes.JMP:
                    return 1;

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

                case OpCodes.ISF:
                case OpCodes.IST:
                case OpCodes.ISFC:
                case OpCodes.ISTC:
                    return 3;

                //case OpCodes.LOOP:
                //case OpCodes.ILOOP:
                case OpCodes.FORL:
                case OpCodes.IFORL:
                case OpCodes.FORI:
                case OpCodes.JFORI:
                case OpCodes.IITERL:
                case OpCodes.ITERL:
                    return 4;

                case OpCodes.UCLO: //apparently this has a jump target. See luajit bytecode ref for more details.
                    return 5;

                default:
                    return -1;
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
            else
                return 0;
        }
    }
}
