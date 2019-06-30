using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dec.Structures;

namespace Luajit_Decompiler.dec
{
    class DecPrototypes
    {
        public string source; //source code from the decompiled file.

        /// <summary>
        /// Decompiles an entire file's prototypes.
        /// </summary>
        /// <param name="name">Name of the entire file.</param>
        /// <param name="pts">List containing that file's prototypes.</param>
        public DecPrototypes(string name, List<Prototype> pts)
        {
            int tabLevel = 0;
            StringBuilder res = new StringBuilder();
            res.AppendLine("--Lua File Name: " + name);
            for (int i = pts.Count; i > 0; i--) //We go backwards here because the 'main' proto is always the last one and will have the most prototype children.
                res.AppendLine(DecPT(GenId(pts[i - 1]), pts[i - 1], ref tabLevel));
            source = res.ToString();
        }
        /// <summary>
        /// Decompiles an individual prototype and converts it to lua source.
        /// </summary>
        /// <param name="id">An ID to organize prototypes in the lua source file.</param>
        /// <param name="pt">The prototype to decompile.</param>
        /// <param name="tabLevel">How many tabs to indent by.</param>
        /// <returns></returns>
        private string DecPT(string id, Prototype pt, ref int tabLevel)
        {
            int bciOffset = 0; //offset for this prototype's bytecode instructions.
            StringBuilder result = new StringBuilder();

            foreach(BytecodeInstruction bci in pt.bytecodeInstructions)
            {
                switch(bci.opcode)
                {
                    case OpCodes.ISLT:
                    case OpCodes.ISGE:
                    case OpCodes.ISLE:
                    case OpCodes.ISGT:
                    case OpCodes.ISEQV:
                    case OpCodes.ISNEV:
                    case OpCodes.ISEQS:
                    case OpCodes.ISNES:
                    case OpCodes.ISEQN:
                    case OpCodes.ISNEN:
                    case OpCodes.ISEQP:
                    case OpCodes.ISNEP:
                        IfSt ifst = new IfSt(pt, ref bciOffset, ref tabLevel); //handle jumps in class.
                        result.AppendLine(ifst.ToString());
                        bciOffset++;
                        break;
                    default: //skip bytecode instruction as default.
                        bciOffset++;
                        continue;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Generates a prototype ID for naming functions: function ID( )
        /// </summary>
        /// <returns></returns>
        private string GenId(Prototype pt)
        {
            //For now we use the prototype index. if index == 0, it is the "main" prototype.
            if (pt.index == 0)
                return "main";
            else
                return "prototype_" + pt.index + "_" + pt.GetIdFromHeader();
        }
    }
}
