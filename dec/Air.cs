using Luajit_Decompiler.dec.data;

namespace Luajit_Decompiler.dec
{
    //[e]{Block}[e](condition Block | Block)

    //[e] = marking for "end" at the beginning of the block or at the end of the block.
    //{Block} = instructions up until the comparison.
    //condition = a op b returns True or False
    //Block | Block = true block, false block if condition
    class Air
    {
        public Block block;
        public Block trueBlock;
        public Block falseBlock;
        public Condition condition;
        private byte end;

        public Air(Block block, Block trueBlock, Block falseBlock, Condition condition)
        {
            this.block = block;
            this.trueBlock = trueBlock;
            this.falseBlock = falseBlock;
            this.condition = condition;
            end = 0;
        }

        public override string ToString()
        {
            string eb = IsMarkedForEndAtBeginning() ? "e" : "";
            string ee = IsMarkedForEndAtEnd() ? "e" : "";
            return eb + "[" + block.index + "]" + ee + "(" + condition.ToString() + trueBlock.index + " | " + falseBlock.index + ")";
        }

        public void MarkEndAtBeginningOfBlock()
        {
            end |= 1;
        }

        public void MarkEndAtEndOfBlock()
        {
            end |= 2;
        }

        public bool IsMarkedForEndAtBeginning()
        {
            return (end & 1) == 1;
        }

        public bool IsMarkedForEndAtEnd()
        {
            return (end & 2) == 1;
        }
    }
}
