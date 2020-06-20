using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.gir;
using Luajit_Decompiler.dis;
using System.Collections.Generic;
using Luajit_Decompiler.dis.consts;

namespace Luajit_Decompiler.dec.tluastates
{
    public enum State
    {
        init,
        check,
        setslot, //operation involving the setting of a slot.
        binop, // + - * / etc... 
        eval,
        loop
    }

    class TLuaState
    {
        public Prototype pt; //reference to the prototype we are working with
        public Cfg cfg; //reference to a prototype's control flow graph.
        public List<Block> blocks; //reference to a prototype's blocks which contain its integrated instructions.
        public int indent; //current nest level. aka the # of tab indentations. Fetch this from the control flow graph.
        //public State current; //current state
        public int[] slots = new int[3]; //0, 1, 2 are the slots in luajit. They act sort of like registers as temporary memory locations for an operation to use.
        public readonly BaseConstant[] _G; //reference to the prototype's global constants table.   
        public readonly BaseConstant[] _U; //value of upvalues in each prototype.

        //might want to keep a mapping of variables to values when they are discovered at execution.

        public TLuaState(ref Prototype pt, ref Cfg cfg, ref List<Block> blocks)
        {
            this.pt = pt;
            this.cfg = cfg;
            this.blocks = blocks;
            //current = State.init;
            _G = pt.constantsSection.ToArray();
            _U = GetUpvalues().ToArray(); //says that _U will always be null for some reason....wat...
        }

        private List<BaseConstant> GetUpvalues()
        {
            List<BaseConstant> result = new List<BaseConstant>();
            foreach (UpValue uv in pt.upvalues)
                result.Add(RecursiveGetUpvalue(pt, uv));
            return result;
        }

        private BaseConstant RecursiveGetUpvalue(Prototype pt, UpValue uv)
        {
            if (uv.tableLocation == 192)
                return pt.parent.constantsSection[uv.tableIndex];
            return RecursiveGetUpvalue(pt.parent, pt.parent.upvalues[uv.tableIndex]);
        }
    }
}
