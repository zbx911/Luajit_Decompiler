using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;
using Luajit_Decompiler.dis.Constants;

namespace Luajit_Decompiler.dec.gir
{
    class CNode : INode
    {
        public int Type { get { return 0; } }
        public INode Next { get; set; } //main code path.
        public Block code;
        private StringBuilder source = new StringBuilder();

        /// <summary>
        /// Creates a code node given the indicies of a block.
        /// </summary>
        /// <param name="sIndex">Starting index for a block.</param>
        /// <param name="eIndex">Ending index for the block which is also the start of the next block.</param>
        public CNode(Block code)
        {
            this.code = code;
            foreach(BytecodeInstruction bci in code.bcis)
            {
                switch(bci.opcode)
                {
                    case OpCodes.KSHORT:
                        VarSetInt(bci);
                        break;
                    default:
                        break;
                }
            }
        }

        private void VarSetInt(BytecodeInstruction bci)
        {
            PtGraph.variables[bci.registers[0]] = new CInt(bci.registers[1]);
            VarSet(bci);
        }
        
        private void VarSet(BytecodeInstruction bci)
        {
            source.AppendLine("temp" + (bci.registers[0] + 1) + " = " + bci.registers[1]);
        }

        public override string ToString()
        {
            return source.ToString();
        }
    }
}
