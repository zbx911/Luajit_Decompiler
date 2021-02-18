using System.Collections.Generic;
using System.Linq;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.data;

namespace Luajit_Decompiler.dec
{
    class DecompilePrototype
    {
        private Prototype pt;
        private readonly List<BytecodeInstruction> ptBcis;
        private List<Jump> jumps;
        private List<Block> blocks;
        private List<Air> airs;

        public DecompilePrototype(Prototype pt)
        {
            this.pt = pt;
            ptBcis = pt.bcis;
            jumps = new List<Jump>();
            blocks = new List<Block>();
            airs = new List<Air>();
        }

        public void StartProtoDecomp()
        {
            BlockPrototype();
            ConstructAirs();
            //DebugBlockPrototype();
        }

        private void BlockPrototype()
        {
            AddTopOfFileJump();
            FindAllJumps();
            FinalizeBlocks(GetJumpTargets());
            SetBlockTargets();
            DebugBlockPrototype();
        }

        private void ConstructAirs()
        {
            for (int i = 0; i < blocks.Count; i++)
                airs.Add(BuildAirFromBlock(blocks[i]));
        }

        private Air BuildAirFromBlock(Block b)
        {
            Condition condi = new Condition(pt, b.GetConditional());
            Block tBlock;
            Block fBlock;
            return null;
        }

        private void DebugBlockPrototype()
        {
            FileManager.ClearDebug();
            FileManager.WriteDebug("Bci total: " + pt.bcis.Count + " From Index: 0-" + (pt.bcis.Count - 1) + "\n\n");
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
                int check = IdentifyJumpOrReturn(ptBcis[i]);
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

        public static int IdentifyJumpOrReturn(BytecodeInstruction bci)
        {
            int op = (int)bci.opcode;
            if (op > 68 && op <= 72) //return op
                return 0;
            if (op == 84) //JMP
                return 1;
            if (op >= 0 && op <= 15) //comparison op
                return 3;
            if (op > 72 && op <= 82) //for loop or iterator loop
                return 4;
            if (op == 48) //UCLO
                return 5;
            return -1; //not jump or return.
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
