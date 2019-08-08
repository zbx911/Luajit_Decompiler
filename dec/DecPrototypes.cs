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
        public static Prototype pt; //reference to current prototype.
        public static List<Jump> jumps; //jumps and their associated targets.
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
                pt = pts[i - 1];
                BlockPrototype(pt.bytecodeInstructions);

                #region debugging
                StringBuilder dbg = new StringBuilder();

                foreach(Jump j in jumps)
                {
                    dbg.AppendLine("Jump@" + j.index + " Type: " + j.jumpType + " Block Starts: " + j.target.sIndex);
                }

                FileManager.WriteDebug(dbg.ToString());
                #endregion
            }
        }

        private void BlockPrototype(List<BytecodeInstruction> ptBcis)
        {
            //find condi and jump treat them as jumps.
            jumps = new List<Jump>();
            int name = 0;
            for(int i = 0; i < ptBcis.Count; i++)
            {
                int check = CheckCJR(ptBcis[i]);
                if (check == 1 || check == 3) //jmp or comparison
                {
                    Jump j = new Jump(ptBcis[i], check, name); //TODO: check if we need to merge jumps. and merge jumps.
                    jumps.Add(j);
                    name++;
                }
            }
        }

        /// <summary>
        /// Check for Condi, Jump, or Ret opcodes.
        /// </summary>
        /// <param name="bci"></param>
        /// <returns></returns>
        public static int CheckCJR(BytecodeInstruction bci)
        {
            switch(bci.opcode)
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
                    return 3; //conditional
                default:
                    return -1; //not condi/jmp/ret
            }
        }

        public override string ToString()
        {
            return fileSource.ToString();
        }
    }
}
