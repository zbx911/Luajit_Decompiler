using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;
using Luajit_Decompiler.dis.Constants;

namespace Luajit_Decompiler.dec
{
    class DecPrototypes
    {
        private StringBuilder fileSource = new StringBuilder(); //source code for the entire file.

        #region Per Prototype
        private Prototype pt; //reference to current prototype.
        private List<Jump> jumps; //jumps and their associated targets.
        private List<Block> blocks; //all blocks of a prototype.
        #endregion

        /// <summary>
        /// Decompiles an entire file's prototypes.
        /// </summary>
        /// <param name="name">Name of the entire file.</param>
        /// <param name="pts">List containing that file's prototypes.</param>
        public DecPrototypes(string name, List<Prototype> pts)
        {
            StringBuilder res = new StringBuilder();
            res.AppendLine("--Lua File Name: " + name);
            for (int i = pts.Count; i > 0; i--) //We go backwards here because the 'main' proto is always the last one and will have the most prototype children.
            {
                jumps = new List<Jump>();
                blocks = new List<Block>();
                pt = pts[i - 1];
                int start = 0;
                BlockifyPT(ref start, pt.bytecodeInstructions.Count);

                #region debugging
                StringBuilder dbg = new StringBuilder();
                foreach (Block b in blocks)
                {
                    dbg.AppendLine(b.label + " ->\r\n");
                    dbg.AppendLine(b.ToString());
                    dbg.AppendLine("end " + b.label + "\r\n");
                }
                FileManager.WriteDebug(dbg.ToString());
                #endregion
            }
        }

        /// <summary>
        /// Recursively separates the prototypes into blocks. 
        /// These blocks need to be refined/simplified as some blocks will contain JMPs and other blocks. 
        /// The final block will be a block that is basically the entire prototype minus the return statement.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void BlockifyPT(ref int start, int end)
        {
            Block b = new Block(pt); //where it starts will be the name
            b.startB = start;
            while(start < end)
            {
                BytecodeInstruction bci = pt.bytecodeInstructions[start];
                int check = IsJumpOrRet(bci);
                if (check == 1)
                {
                    Jump j = new Jump(bci, start);
                    jumps.Add(j);
                    start++; //start of next block
                    BlockifyPT(ref start, j.distance + start);
                }
                else if (check == 2) //a return
                    break;
                else
                    start++;
            }
            b.endB = start; //base cases will break the loop and return since no JMPs. exclusive bci fetching so no -1
            b.GetBcis();
            blocks.Add(b);
            return;
        }

        public static int IsJumpOrRet(BytecodeInstruction bci)
        {
            switch(bci.opcode)
            {
                case OpCodes.JMP:
                    return 1;
                case OpCodes.RET:
                case OpCodes.RET0:
                case OpCodes.RET1:
                case OpCodes.RETM:
                    return 2;
                default:
                    return 0;
            }
        }

        public override string ToString()
        {
            return fileSource.ToString();
        }
    }
}
