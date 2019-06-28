using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;

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
                res.AppendLine(DecPT(GenId(pts[i]), pts[i], ref tabLevel));
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
            StringBuilder res = new StringBuilder();

            //TODO: Implement method.

            return res.ToString();
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
