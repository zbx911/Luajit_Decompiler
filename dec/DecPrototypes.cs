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
        private StringBuilder fileSource = new StringBuilder(); //source code for the entire file.

        /// <summary>
        /// Decompiles an entire file's prototypes. Entry point for DPT class.
        /// </summary>
        /// <param name="name">Name of the entire file.</param>
        /// <param name="pts">List containing that file's prototypes.</param>
        public DecPrototypes(string name, List<Prototype> pts)
        {
            StringBuilder res = new StringBuilder();
            res.AppendLine("--Lua File Name: " + name);
            for (int i = pts.Count; i > 0; i--) //We go backwards here because the 'main' proto is always the last one and will have the most prototype children. In addition, it will contain highest tier of upvalues.
            {
                DPT dpt = new DPT(pts[i - 1]);

                #region debugging
                StringBuilder dbg = new StringBuilder();
                foreach (Jump j in dpt.jumps)
                {
                    //dbg.AppendLine("Jump@" + j.index + " Type: " + j.jumpType + " Block Starts: " + j.target.sIndex);
                    dbg.AppendLine("Jump@" + j.index + "; Target =>\r\n" + j.target.ToString());
                }
                FileManager.ClearDebug();
                FileManager.WriteDebug(dbg.ToString());
                #endregion
            }
        }

        public override string ToString()
        {
            return fileSource.ToString();
        }
    }
}